using choir_music_system.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.MusicLibrary;

public class ViewModel : PageModel
{
    private readonly ChoirDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ViewModel(
        ChoirDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var Song = await _context.Songs
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (Song is null)
        {
            return NotFound();
        }

        var fullPath = Path.Combine(
            _environment.ContentRootPath,
            Song.PdfPath
        );

        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);

        return File(
            fileBytes,
            "application/pdf"
        );
    }
}