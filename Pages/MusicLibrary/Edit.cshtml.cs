using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.MusicLibrary;

public class EditModel : PageModel
{
    private readonly ChoirDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public EditModel(
        ChoirDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [BindProperty]
    public MusicSheet MusicSheet { get; set; } = new();

    [BindProperty]
    public IFormFile? PdfFile { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

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
        var existingMusicSheet = await _context.MusicSheets
            .FirstOrDefaultAsync(x => x.Id == MusicSheet.Id);

        if (existingMusicSheet is null)
        {
            return NotFound();
        }

        if (PdfFile is not null && PdfFile.Length > 0)
        {
            const long maxFileSize = 25 * 1024 * 1024;

            if (PdfFile.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(PdfFile),
                    "The PDF file must be 25 MB or smaller."
                );

                return Page();
            }
            var extension = Path.GetExtension(PdfFile.FileName);

            if (!string.Equals(
                extension,
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(PdfFile),
                    "Only PDF files are allowed."
                );

                return Page();
            }

            var storageFolder = Path.Combine(
                _environment.ContentRootPath,
                "Storage",
                "MusicSheets"
            );

            Directory.CreateDirectory(storageFolder);

            var newStoredFileName = $"{Guid.NewGuid():N}.pdf";

            var newFullPath = Path.Combine(
                storageFolder,
                newStoredFileName
            );

            await using (var stream = new FileStream(
                newFullPath,
                FileMode.Create))
            {
                await PdfFile.CopyToAsync(stream);
            }

            var oldFullPath = Path.Combine(
                _environment.ContentRootPath,
                existingMusicSheet.PdfPath
            );

            if (System.IO.File.Exists(oldFullPath))
            {
                System.IO.File.Delete(oldFullPath);
            }

            existingMusicSheet.PdfFileName = PdfFile.FileName;

            existingMusicSheet.PdfPath = Path.Combine(
                "Storage",
                "MusicSheets",
                newStoredFileName
            );
        }

        existingMusicSheet.Title = MusicSheet.Title;
        existingMusicSheet.SuggestedMassPart = MusicSheet.SuggestedMassPart;
        existingMusicSheet.Composer = MusicSheet.Composer;
        existingMusicSheet.Arrangement = MusicSheet.Arrangement;
        existingMusicSheet.Key = MusicSheet.Key;
        existingMusicSheet.Notes = MusicSheet.Notes;
        existingMusicSheet.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(ReturnUrl) &&
    Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return RedirectToPage("Index");
    }
}