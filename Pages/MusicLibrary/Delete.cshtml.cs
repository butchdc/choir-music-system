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
    public MusicSheet MusicSheet { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var musicSheet = await _context.MusicSheets
            .FirstOrDefaultAsync(x => x.Id == id);

        if (musicSheet is null)
        {
            return NotFound();
        }

        MusicSheet = musicSheet;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var musicSheet = await _context.MusicSheets
            .FirstOrDefaultAsync(x => x.Id == MusicSheet.Id);

        if (musicSheet is null)
        {
            return NotFound();
        }

        var fullPath = Path.Combine(
            _environment.ContentRootPath,
            musicSheet.PdfPath
        );

        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }

        _context.MusicSheets.Remove(musicSheet);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}