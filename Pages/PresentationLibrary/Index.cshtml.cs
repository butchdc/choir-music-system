using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.PresentationLibrary;

public class IndexModel : PageModel
{
    private readonly ChoirDbContext _context;

    public IndexModel(ChoirDbContext context)
    {
        _context = context;
    }

    public IList<PresentationItem> Items { get; set; }
        = new List<PresentationItem>();

    public async Task OnGetAsync()
    {
        Items = await _context.PresentationItems
            .Where(x => x.IsActive)
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Title)
            .ToListAsync();
    }
}