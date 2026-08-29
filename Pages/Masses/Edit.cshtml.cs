using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.Masses;

public class EditModel : PageModel
{
    private readonly ChoirDbContext _context;

    public EditModel(ChoirDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Mass Mass { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var mass = await _context.Masses.FindAsync(id);

        if (mass is null)
            return NotFound();

        Mass = mass;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var existing = await _context.Masses.FindAsync(Mass.Id);

        if (existing is null)
            return NotFound();

        existing.Name = Mass.Name;
        existing.MassDate = Mass.MassDate;
        existing.Notes = Mass.Notes;
        existing.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}