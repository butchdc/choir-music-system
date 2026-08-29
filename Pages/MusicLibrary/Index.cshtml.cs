using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.MusicLibrary;

public class IndexModel : PageModel
{
    private readonly ChoirDbContext _context;

    public IndexModel(ChoirDbContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public IList<MusicSheet> MusicSheets { get; set; }
        = new List<MusicSheet>();

    public string? ActivePart { get; set; }

    public async Task OnGetAsync(string? part)
    {
        ActivePart = part;

        var query = _context.MusicSheets
            .Where(x => x.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(part))
        {
            if (part == "Not specified")
            {
                query = query.Where(x =>
                    x.SuggestedMassPart == null ||
                    x.SuggestedMassPart == "");
            }
            else
            {
                query = query.Where(x =>
                    x.SuggestedMassPart == part);
            }
        }

        MusicSheets = await query
            .OrderBy(x => x.Title)
            .ToListAsync();
    }
}