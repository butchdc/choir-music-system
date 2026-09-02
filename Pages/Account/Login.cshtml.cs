using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace choir_music_system.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    [TempData]
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }

        return Page();
    }

    public IActionResult OnPostGoogle(string? returnUrl = null)
    {
        var redirectUrl = Url.Page(
            "/Account/GoogleCallback",
            pageHandler: null,
            values: new
            {
                returnUrl
            },
            protocol: Request.Scheme);

        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUrl
        };

        return Challenge(
            properties,
            GoogleDefaults.AuthenticationScheme);
    }
}