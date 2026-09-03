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

    [BindProperty]
    public IFormFile? CustomPresentationFile { get; set; }

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

    public async Task<IActionResult> OnGetDownloadCustomPresentationAsync(
        int id)
    {
        var song = await _context.Songs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (song is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(
                song.CustomPresentationPath))
        {
            return NotFound();
        }

        var fullPath = Path.Combine(
            _environment.ContentRootPath,
            song.CustomPresentationPath);

        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }

        var downloadName =
            !string.IsNullOrWhiteSpace(
                song.CustomPresentationFileName)
                ? song.CustomPresentationFileName
                : $"{song.Title}.pptx";

        return PhysicalFile(
            fullPath,
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            downloadName);
    }


    public async Task<IActionResult> OnPostRemoveCustomPresentationAsync()
    {
        var song = await _context.Songs
            .FirstOrDefaultAsync(x => x.Id == Song.Id);

        if (song is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(
                song.CustomPresentationPath))
        {
            var fullPath = Path.Combine(
                _environment.ContentRootPath,
                song.CustomPresentationPath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        song.CustomPresentationFileName = null;
        song.CustomPresentationPath = null;
        song.CustomPresentationUpdatedDate = null;
        song.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToPage(
            new
            {
                id = song.Id,
                ReturnUrl
            });
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
        if (CustomPresentationFile is not null &&
    CustomPresentationFile.Length > 0)
        {
            const long maxPresentationSize =
                50 * 1024 * 1024;

            if (CustomPresentationFile.Length >
                maxPresentationSize)
            {
                ModelState.AddModelError(
                    nameof(CustomPresentationFile),
                    "The PowerPoint file must be 50 MB or smaller."
                );

                Song.CustomPresentationFileName =
                    existingSong.CustomPresentationFileName;

                Song.CustomPresentationPath =
                    existingSong.CustomPresentationPath;

                Song.CustomPresentationUpdatedDate =
                    existingSong.CustomPresentationUpdatedDate;

                return Page();
            }

            var extension =
                Path.GetExtension(
                    CustomPresentationFile.FileName);

            if (!string.Equals(
                    extension,
                    ".pptx",
                    StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(CustomPresentationFile),
                    "Only PowerPoint .pptx files are allowed."
                );

                Song.CustomPresentationFileName =
                    existingSong.CustomPresentationFileName;

                Song.CustomPresentationPath =
                    existingSong.CustomPresentationPath;

                Song.CustomPresentationUpdatedDate =
                    existingSong.CustomPresentationUpdatedDate;

                return Page();
            }

            var presentationFolder =
                Path.Combine(
                    _environment.ContentRootPath,
                    "Storage",
                    "SongPresentations");

            Directory.CreateDirectory(
                presentationFolder);

            var storedFileName =
                $"{Guid.NewGuid():N}.pptx";

            var fullPath =
                Path.Combine(
                    presentationFolder,
                    storedFileName);

            await using (
                var stream =
                    new FileStream(
                        fullPath,
                        FileMode.Create))
            {
                await CustomPresentationFile
                    .CopyToAsync(stream);
            }

            // Remove the previous custom presentation
            // only after the new file has saved successfully.
            if (!string.IsNullOrWhiteSpace(
                    existingSong.CustomPresentationPath))
            {
                var oldPresentationPath =
                    Path.Combine(
                        _environment.ContentRootPath,
                        existingSong.CustomPresentationPath);

                if (System.IO.File.Exists(
                        oldPresentationPath))
                {
                    System.IO.File.Delete(
                        oldPresentationPath);
                }
            }

            existingSong.CustomPresentationFileName =
                CustomPresentationFile.FileName;

            existingSong.CustomPresentationPath =
                Path.Combine(
                    "Storage",
                    "SongPresentations",
                    storedFileName);

            existingSong.CustomPresentationUpdatedDate =
                DateTime.UtcNow;
        }
        existingSong.SuggestedMassPart = Song.SuggestedMassPart;
        existingSong.Composer = Song.Composer;
        existingSong.Arrangement = Song.Arrangement;
        existingSong.Key = Song.Key;
        existingSong.Notes = Song.Notes;
        existingSong.OneLicenseNumber = Song.OneLicenseNumber;
        existingSong.Publisher = Song.Publisher;
        existingSong.CopyrightText = Song.CopyrightText;
        existingSong.PresentationLyrics = Song.PresentationLyrics;
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