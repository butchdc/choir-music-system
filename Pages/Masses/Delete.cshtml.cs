using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace choir_music_system.Pages.Masses;

[Authorize(Policy = "AdminOnly")]
public class DeleteModel : PageModel
{
    private readonly ChoirDbContext _context;

    public DeleteModel(ChoirDbContext context)
    {
        _context = context;
    }

    public Mass Mass { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var mass = await _context.Masses.FindAsync(id);

        if (mass is null)
            return NotFound();

        Mass = mass;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var mass = await _context.Masses.FindAsync(id);

        if (mass is null)
            return NotFound();

        // Remove the music selections first.
        var selections = await _context.MassSongs
            .Where(x => x.MassId == id)
            .ToListAsync();

        _context.MassSongs.RemoveRange(selections);
        _context.Masses.Remove(mass);

        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}