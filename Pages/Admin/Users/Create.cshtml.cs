using System.ComponentModel.DataAnnotations;
using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.Admin.Users;

[Authorize(Policy = "AdminOnly")]
public class CreateModel : PageModel
{
    private readonly ChoirDbContext _db;

    public CreateModel(ChoirDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public string Role { get; set; } = "Member";

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Role != "Admin" && Role != "Member")
        {
            ModelState.AddModelError(nameof(Role), "Invalid role.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();

        var exists = await _db.AppUsers
            .AnyAsync(x => x.NormalizedEmail == normalizedEmail);

        if (exists)
        {
            ModelState.AddModelError(
                nameof(Email),
                "This email address already has access.");

            return Page();
        }

        var user = new AppUser
        {
            Email = email,
            NormalizedEmail = normalizedEmail,
            Role = Role,
            IsActive = true,
            InvitedAt = DateTime.UtcNow,
            InvitedBy = User.Identity?.Name
        };

        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}