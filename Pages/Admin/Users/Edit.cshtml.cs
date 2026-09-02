using choir_music_system.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.Admin.Users;

[Authorize(Policy = "AdminOnly")]
public class EditModel : PageModel
{
    private readonly ChoirDbContext _db;

    public EditModel(ChoirDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Role { get; set; } = "Member";

    [BindProperty]
    public bool IsActive { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _db.AppUsers.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        Id = user.Id;
        Email = user.Email;
        Role = user.Role;
        IsActive = user.IsActive;

        return Page();
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

        var user = await _db.AppUsers
            .SingleOrDefaultAsync(x => x.Id == Id);

        if (user == null)
        {
            return NotFound();
        }

        // If this user is currently an active Admin, make sure
        // another active Admin exists before removing their Admin access.
        if (user.Role == "Admin" &&
            user.IsActive &&
            (Role != "Admin" || !IsActive))
        {
            var otherActiveAdmins = await _db.AppUsers
                .AnyAsync(x =>
                    x.Id != user.Id &&
                    x.Role == "Admin" &&
                    x.IsActive);

            if (!otherActiveAdmins)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "You cannot disable or demote the last active Admin.");

                Email = user.Email;
                Role = user.Role;
                IsActive = user.IsActive;

                return Page();
            }
        }

        user.Role = Role;
        user.IsActive = IsActive;

        await _db.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}