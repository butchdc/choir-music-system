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

    public string? ActiveFilter { get; set; }

    public async Task OnGetAsync(string? filter)
    {
        ActiveFilter = filter;

        var today = DateTime.Today;

        var query = _context.Masses
            .Include(x => x.Songs)
                .ThenInclude(x => x.Song)
            .AsQueryable();

        switch (filter?.ToLowerInvariant())
        {
            case "upcoming":
                query = query.Where(x => x.MassDate >= today);
                break;

            case "past":
                query = query.Where(x => x.MassDate < today);
                break;
        }

        Masses = await query
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

        var sourceSelections = await _context.MassSongs
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
            _context.MassSongs.Add(
                new MassSong
                {
                    MassId = newMass.Id,
                    SongId = selection.SongId,
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