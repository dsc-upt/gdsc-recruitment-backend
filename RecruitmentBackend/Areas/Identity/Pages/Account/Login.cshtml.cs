using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RecruitmentBackend.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    public async Task OnGet(string returnUrl = "/")
    {
        await HttpContext.ChallengeAsync("Google", new AuthenticationProperties
        {
            RedirectUri = Url.Page("./ExternalLogin", "Callback", new { returnUrl }),
        });
    }
}
