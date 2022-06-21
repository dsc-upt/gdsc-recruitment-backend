using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace RecruitmentBackend.Pages.Identity
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        public IActionResult OnGetAsync(string? returnUrl)
        {
            // Request a redirect to the external login provider.
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Page("./Login",
                    pageHandler: "Callback",
                    values: new { returnUrl }),
            };
            return new ChallengeResult("Google", authenticationProperties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl, string? remoteError)
        {
            // Get the information about the user from the external login provider
            var googleUser = User.Identities.FirstOrDefault();
            if (googleUser is not { IsAuthenticated: true })
            {
                Console.WriteLine("Not authenticated");
                return LocalRedirect("/");
            }

            Console.WriteLine(JsonConvert.SerializeObject(googleUser, Formatting.Indented,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                RedirectUri = Request.Host.Value
            };
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(googleUser),
                authProperties);

            Console.WriteLine("Authenticated");
            return LocalRedirect("/");
        }
    }
}
