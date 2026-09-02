using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace choir_music_system.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class BreakGlassHashModel : PageModel
{
    private readonly IPasswordHasher<object> _passwordHasher;

    public BreakGlassHashModel(
        IPasswordHasher<object> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? GeneratedHash { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            ModelState.AddModelError(
                nameof(Password),
                "Enter a password.");

            return Page();
        }

        GeneratedHash =
            _passwordHasher.HashPassword(
                new object(),
                Password);

        // Do not retain the plaintext password.
        Password = string.Empty;

        return Page();
    }
}