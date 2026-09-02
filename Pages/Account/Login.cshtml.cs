using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly ChoirDbContext _context;

    public LoginModel(
        ChoirDbContext context)
    {
        _context = context;
    }

    [TempData]
    public string? ErrorMessage { get; set; }

    public IList<Mass> UpcomingMasses { get; set; }
        = new List<Mass>();

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }

        UpcomingMasses =
            await _context.Masses
                .Where(x =>
                    x.MassDate >= DateTime.Now)
                .OrderBy(x =>
                    x.MassDate)
                .Take(5)
                .ToListAsync();

        return Page();
    }

    public IActionResult OnPostGoogle(
        string? returnUrl = null)
    {
        var redirectUrl =
            Url.Page(
                "/Account/GoogleCallback",
                pageHandler: null,
                values: new
                {
                    returnUrl
                },
                protocol: Request.Scheme);

        var properties =
            new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

        return Challenge(
            properties,
            GoogleDefaults.AuthenticationScheme);
    }
}