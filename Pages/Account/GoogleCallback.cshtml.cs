using System.Security.Claims;
using choir_music_system.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.Account;

[AllowAnonymous]
public class GoogleCallbackModel : PageModel
{
    private readonly ChoirDbContext _db;

    public GoogleCallbackModel(ChoirDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        var externalResult =
            await HttpContext.AuthenticateAsync("External");

        if (!externalResult.Succeeded ||
            externalResult.Principal == null)
        {
            return RedirectToPage(
                "/Account/Login",
                new
                {
                    error = "Google sign-in could not be completed."
                });
        }

        var email =
            externalResult.Principal.FindFirstValue(
                ClaimTypes.Email);

        var name =
            externalResult.Principal.FindFirstValue(
                ClaimTypes.Name);

        var picture =
            externalResult.Principal.FindFirstValue(
                "urn:google:picture");

        if (string.IsNullOrWhiteSpace(email))
        {
            await HttpContext.SignOutAsync("External");

            return RedirectToPage("/Account/Login");
        }

        var normalizedEmail =
            email.Trim().ToUpperInvariant();

        var appUser = await _db.AppUsers
            .SingleOrDefaultAsync(x =>
                x.NormalizedEmail == normalizedEmail);

        if (appUser == null || !appUser.IsActive)
        {
            await HttpContext.SignOutAsync("External");

            TempData["ErrorMessage"] =
                "Your Google account has not been invited to this Choir Music System.";

            return RedirectToPage("/Account/Login");
        }

        appUser.LastLoginAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(name))
        {
            appUser.DisplayName = name;
        }

        await _db.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, appUser.Id.ToString()),
            new(ClaimTypes.Email, appUser.Email),
            new(ClaimTypes.Name,
                appUser.DisplayName ?? appUser.Email),
            new(ClaimTypes.Role, appUser.Role)
        };
        
        if (!string.IsNullOrWhiteSpace(picture))
        {
            claims.Add(
                new Claim(
                    "profile_picture",
                    picture));
        }

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignOutAsync("External");

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true
            });

        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToPage("/Index");
    }
}