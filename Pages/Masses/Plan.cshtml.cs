using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.Masses;

public class PlanModel : PageModel
{
    private readonly ChoirDbContext _context;

    public PlanModel(ChoirDbContext context)
    {
        _context = context;
    }

    public Mass Mass { get; set; } = null!;

    public IList<Song> Songs { get; set; }
        = new List<Song>();

    [BindProperty]
    public List<MassPartSelection> Selections { get; set; }
        = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var mass = await _context.Masses
            .FirstOrDefaultAsync(x => x.Id == id);

        if (mass is null)
        {
            return NotFound();
        }

        Mass = mass;

        Songs = await _context.Songs
            .Where(x => x.IsActive)
            .OrderBy(x => x.Title)
            .ToListAsync();

        var existingSelections = await _context.MassSongs
            .Where(x => x.MassId == id)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        Selections = new List<MassPartSelection>();

        var displayOrder = 1;

        foreach (var part in GetMassParts())
        {
            var partSelections = existingSelections
                .Where(x => x.MassPart == part)
                .OrderBy(x => x.DisplayOrder)
                .ToList();

            if (partSelections.Count == 0)
            {
                Selections.Add(new MassPartSelection
                {
                    MassPart = part,
                    SongId = null,
                    DisplayOrder = displayOrder++
                });

                continue;
            }

            foreach (var existing in partSelections)
            {
                Selections.Add(new MassPartSelection
                {
                    MassPart = part,
                    SongId = existing.SongId,
                    DisplayOrder = displayOrder++
                });
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var mass = await _context.Masses
            .FirstOrDefaultAsync(x => x.Id == id);

        if (mass is null)
        {
            return NotFound();
        }

        var existing = await _context.MassSongs
            .Where(x => x.MassId == id)
            .ToListAsync();

        _context.MassSongs.RemoveRange(existing);

        var displayOrder = 1;

        foreach (var selection in Selections)
        {
            if (!selection.SongId.HasValue)
            {
                continue;
            }

            _context.MassSongs.Add(new MassSong
            {
                MassId = id,
                SongId = selection.SongId.Value,
                MassPart = selection.MassPart,
                DisplayOrder = displayOrder++
            });
        }

        mass.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }

    private static List<string> GetMassParts()
    {
        return new List<string>
    {
        "Entrance",
        "Kyrie",
        "Gloria",
        "Psalm",
        "Alleluia",
        "Offertory",
        "Holy",
        "Memorial Acclamation",
        "Amen",
        "Our Father",
        "Lamb of God",
        "Communion",
        "Recessional"
    };
    }

    public class MassPartSelection
    {
        public string MassPart { get; set; } = string.Empty;

        public int? SongId { get; set; }

        public int DisplayOrder { get; set; }
    }
}