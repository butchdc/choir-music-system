using choir_music_system.Data;
using choir_music_system.Models;
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

    public IList<MusicSheet> MusicSheets { get; set; } = new List<MusicSheet>();

    public async Task OnGetAsync()
    {
        MusicSheets = await _context.MusicSheets
            .Where(x => x.IsActive)
            .OrderBy(x => x.Title)
            .ToListAsync();
    }
}