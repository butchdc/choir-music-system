using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace choir_music_system.Pages.Account;

[AllowAnonymous]
[EnableRateLimiting("EmergencyLogin")]
public class EmergencyModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher<object> _passwordHasher;
    private readonly ILogger<EmergencyModel> _logger;

    public EmergencyModel(
        IConfiguration configuration,
        IPasswordHasher<object> passwordHasher,
        ILogger<EmergencyModel> logger)
    {
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    [BindProperty]
    [Required]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool IsEnabled =>
        _configuration.GetValue<bool>(
            "Security:BreakGlass:Enabled");

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!IsEnabled)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var configuredUsername =
            _configuration["Security:BreakGlass:Username"];

        var configuredHash =
            _configuration["Security:BreakGlass:PasswordHash"];

        if (string.IsNullOrWhiteSpace(configuredUsername) ||
            string.IsNullOrWhiteSpace(configuredHash))
        {

            _logger.LogError(
                "Break-glass login attempted but emergency credentials are not configured. IP: {IpAddress}",
                HttpContext.Connection.RemoteIpAddress);
            ModelState.AddModelError(
                string.Empty,
                "Emergency login is not configured.");

            return Page();
        }

        var usernameMatches =
            string.Equals(
                Username.Trim(),
                configuredUsername,
                StringComparison.Ordinal);

        var passwordResult =
            _passwordHasher.VerifyHashedPassword(
                new object(),
                configuredHash,
                Password);

        if (!usernameMatches ||
            passwordResult == PasswordVerificationResult.Failed)
        {

            _logger.LogWarning(
                "Failed break-glass login attempt. IP: {IpAddress}",
                HttpContext.Connection.RemoteIpAddress);

            ModelState.AddModelError(
                string.Empty,
                "Invalid emergency credentials.");

            return Page();
        }

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                "break-glass"),

            new(
                ClaimTypes.Name,
                "Emergency Administrator"),

            new(
                ClaimTypes.Role,
                "Admin"),

            new(
                "auth_method",
                "break-glass")
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            });

        _logger.LogWarning(
            "Break-glass administrator login succeeded. IP: {IpAddress}",
            HttpContext.Connection.RemoteIpAddress);

        return RedirectToPage("/Index");
    }
}