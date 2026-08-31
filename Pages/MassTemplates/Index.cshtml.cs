using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.MassTemplates;

public class IndexModel : PageModel
{
    private readonly ChoirDbContext _context;

    public IndexModel(ChoirDbContext context)
    {
        _context = context;
    }

    public IList<MassTemplate> Templates { get; set; }
        = new List<MassTemplate>();

    public async Task OnGetAsync()
    {
        Templates = await _context.MassTemplates
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }
}