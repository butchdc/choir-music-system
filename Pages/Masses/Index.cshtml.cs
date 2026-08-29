using choir_music_system.Data;
using choir_music_system.Models;
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
            .OrderByDescending(x => x.MassDate)
            .ToListAsync();
    }
}