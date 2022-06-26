using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using RecruitmentBackend.Features.Users;
using RecruitmentBackend.Utilities;

namespace RecruitmentBackend.Areas.Identity.Pages.Account;

public class ExternalLogin : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IUserStore<User> _userStore;
    private readonly IUserEmailStore<User> _emailStore;
    private const string Provider = "Google";


    public ExternalLogin(SignInManager<User> signInManager, UserManager<User> userManager, IUserStore<User> userStore,
        IEmailSender emailSender)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
    }

    public IActionResult OnGet(string? returnUrl)
    {
        var redirectUrl = Url.Page("./ExternalLogin", "Callback", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(Provider, redirectUrl);
        // Request a redirect to the external login provider.
        return new ChallengeResult(Provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl, string? remoteError)
    {
        if (remoteError != null)
        {
            return GoToErrorPage($"Error from external provider: {remoteError}");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            return GoToErrorPage("Error loading external login information.");
        }

        var isEmailVerified = info.Principal.FindFirstValue(CustomClaimTypes.EmailVerified);
        if (isEmailVerified != "True")
        {
            return GoToErrorPage("Email is not verified");
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (email is null)
        {
            return GoToErrorPage("No email from external provider.");
        }

        var user = await _userManager.FindByEmailAsync(email) ?? new User();

        await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
        await _emailStore.SetEmailAsync(user, email, CancellationToken.None);

        var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
        if (firstName is null)
        {
            return GoToErrorPage("No first name from external provider.");
        }

        var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
        if (lastName is null)
        {
            return GoToErrorPage("No lastName from external provider.");
        }

        var picture = info.Principal.FindFirst("urn:google:picture");
        if (picture is not null)
        {
            await _userManager.AddClaimAsync(user, picture);
        }

        user.FirstName = firstName;
        user.LastName = lastName;

        returnUrl ??= Url.Content("~/");
        var userAlreadyExists = user.SecurityStamp is not null || user.ConcurrencyStamp is not null;
        if (userAlreadyExists)
        {
            return await UpdateUserAndSignIn(user, info, returnUrl);
        }

        user.EmailConfirmed = true;
        var createResult = await _userManager.CreateAsync(user);

        if (createResult.Succeeded)
        {
            return await HandleNewUserSignIn(user, info, returnUrl);
        }

        return GoToErrorPage(GetResultErrorString(createResult));
    }

    public string GetResultErrorString(IdentityResult result)
    {
        var errors = result.Errors.Select(e => e.Description);
        return string.Join("\n", errors);
    }

    private async Task<IActionResult> HandleNewUserSignIn(User user, ExternalLoginInfo info, string returnUrl)
    {
        var loginResult = await _userManager.AddLoginAsync(user, info);
        if (!loginResult.Succeeded)
        {
            return GoToErrorPage(GetResultErrorString(loginResult));
        }

        await _signInManager.SignInAsync(user, false, info.LoginProvider);
        return LocalRedirect(returnUrl);
    }

    private string GetCallbackUrl(string code, string userId)
    {
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var values = new { area = "Identity", userId, code };

        return Url.Page("/Account/ConfirmEmail", null, values, Request.Scheme)!;
    }

    private async Task<IActionResult> UpdateUserAndSignIn(User user, ExternalLoginInfo info, string returnUrl)
    {
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return GoToErrorPage(GetResultErrorString(updateResult));
        }

        // Sign in the user with this external login provider if the user already has a login.
        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, true);
        if (signInResult.Succeeded)
        {
            return LocalRedirect(returnUrl);
        }

        if (signInResult.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }

        if (signInResult.IsNotAllowed)
        {
            return GoToErrorPage("User cannot sign in without a confirmed account.");
        }

        return GoToErrorPage("Error signing in with external login provider.");
    }

    private LocalRedirectResult GoToErrorPage(string? errorMessage)
    {
        return LocalRedirect(GetErrorPageUrl(errorMessage));
    }

    private static string GetErrorPageUrl(string? errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
        {
            return "/error";
        }

        return "/error?errorMessage=" + UrlEncoder.Default.Encode(errorMessage);
    }

    private IUserEmailStore<User> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }

        return (IUserEmailStore<User>)_userStore;
    }
}
