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
    public Song Song { get; set; } = new();

    [BindProperty]
    public IFormFile? PdfFile { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

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
                "Songs"
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
                existingSong.PdfPath
            );

            if (System.IO.File.Exists(oldFullPath))
            {
                System.IO.File.Delete(oldFullPath);
            }

            existingSong.PdfFileName = PdfFile.FileName;

            existingSong.PdfPath = Path.Combine(
                "Storage",
                "Songs",
                newStoredFileName
            );
        }

        existingSong.Title = Song.Title;
        existingSong.SuggestedMassPart = Song.SuggestedMassPart;
        existingSong.Composer = Song.Composer;
        existingSong.Arrangement = Song.Arrangement;
        existingSong.Key = Song.Key;
        existingSong.Notes = Song.Notes;
        existingSong.OneLicenseNumber = Song.OneLicenseNumber;
        existingSong.Publisher = Song.Publisher;
        existingSong.CopyrightText = Song.CopyrightText;
        existingSong.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(ReturnUrl) &&
    Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return RedirectToPage("Index");
    }
}