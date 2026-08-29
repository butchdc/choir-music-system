using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.Masses;

public class IndexModel : PageModel
{
    private readonly ChoirDbContext _context;

    public IndexModel(ChoirDbContext context)
    {
        _context = context;
    }

    public IList<Mass> Masses { get; set; }
        = new List<Mass>();

    public async Task OnGetAsync()
    {
        Masses = await _context.Masses
            .Include(x => x.MusicSheets)
                .ThenInclude(x => x.MusicSheet)
            .OrderByDescending(x => x.MassDate)
            .ToListAsync();
    }

    public async Task<IActionResult> OnGetDuplicateAsync(int id)
    {
        var sourceMass = await _context.Masses
            .FirstOrDefaultAsync(x => x.Id == id);

        if (sourceMass is null)
        {
            return NotFound();
        }

        var sourceSelections = await _context.MassMusicSheets
            .Where(x => x.MassId == id)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        var newMass = new Mass
        {
            Name = $"{sourceMass.Name} Copy",
            MassDate = sourceMass.MassDate.AddDays(7),
            Notes = sourceMass.Notes,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        _context.Masses.Add(newMass);

        await _context.SaveChangesAsync();

        foreach (var selection in sourceSelections)
        {
            _context.MassMusicSheets.Add(
                new MassMusicSheet
                {
                    MassId = newMass.Id,
                    MusicSheetId = selection.MusicSheetId,
                    MassPart = selection.MassPart,
                    DisplayOrder = selection.DisplayOrder
                }
            );
        }

        await _context.SaveChangesAsync();

        return RedirectToPage(
            "Edit",
            new { id = newMass.Id }
        );
    }
}