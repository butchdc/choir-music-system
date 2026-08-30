using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.MusicLibrary;

public class DeleteModel : PageModel
{
    private readonly ChoirDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public DeleteModel(
        ChoirDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [BindProperty]
    public Song Song { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var existingSong = await _context.Songs
            .FirstOrDefaultAsync(x => x.Id == id);

        if (existingSong is null)
        {
            return NotFound();
        }

        Song = existingSong;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var existingSong = await _context.Songs
            .FirstOrDefaultAsync(x => x.Id == Song.Id);

        if (existingSong is null)
        {
            return NotFound();
        }

        var fullPath = Path.Combine(
            _environment.ContentRootPath,
            existingSong.PdfPath
        );

        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }

        _context.Songs.Remove(existingSong);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}