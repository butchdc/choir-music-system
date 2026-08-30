using choir_music_system.Data;
using choir_music_system.Models;
using choir_music_system.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace choir_music_system.Pages.MusicLibrary;

public class CreateModel : PageModel
{
    private readonly ChoirDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly PdfMetadataExtractorService _metadataExtractor;

    public CreateModel(
        ChoirDbContext context,
        IWebHostEnvironment environment,
        PdfMetadataExtractorService metadataExtractor)
    {
        _context = context;
        _environment = environment;
        _metadataExtractor = metadataExtractor;
    }

    [BindProperty]
    public Song Song { get; set; } = new();

    [BindProperty]
    public IFormFile? PdfFile { get; set; }

    [BindProperty]
    public string? PendingPdfFileName { get; set; }

    [BindProperty]
    public string? OriginalPdfFileName { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostDetectAsync()
    {
        if (PdfFile is null || PdfFile.Length == 0)
        {
            ModelState.AddModelError(
                nameof(PdfFile),
                "Please select a PDF file."
            );

            return Page();
        }

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

        var tempFolder = Path.Combine(
            _environment.ContentRootPath,
            "Storage",
            "Temp"
        );

        Directory.CreateDirectory(tempFolder);

        var tempFileName = $"{Guid.NewGuid():N}.pdf";

        var tempFilePath = Path.Combine(
            tempFolder,
            tempFileName
        );

        await using (var stream = new FileStream(
            tempFilePath,
            FileMode.Create))
        {
            await PdfFile.CopyToAsync(stream);
        }

        var metadata = _metadataExtractor.Extract(tempFilePath);

        if (!string.IsNullOrWhiteSpace(metadata.Title))
        {
            Song.Title = metadata.Title;
        }

        PendingPdfFileName = tempFileName;
        OriginalPdfFileName = PdfFile.FileName;

        ModelState.Clear();

        return Page();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        var hasNewPdf = PdfFile is not null && PdfFile.Length > 0;
        var hasPendingPdf = !string.IsNullOrWhiteSpace(PendingPdfFileName);

        if (!hasNewPdf && !hasPendingPdf)
        {
            ModelState.AddModelError(
                nameof(PdfFile),
                "Please select a PDF file."
            );
        }

        if (hasNewPdf)
        {
            const long maxFileSize = 25 * 1024 * 1024;

            if (PdfFile!.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(PdfFile),
                    "The PDF file must be 25 MB or smaller."
                );
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
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var storageFolder = Path.Combine(
            _environment.ContentRootPath,
            "Storage",
            "Songs"
        );

        Directory.CreateDirectory(storageFolder);

        var storedFileName = $"{Guid.NewGuid():N}.pdf";

        var storedFilePath = Path.Combine(
            storageFolder,
            storedFileName
        );

        if (hasNewPdf)
        {
            await using var stream = new FileStream(
                storedFilePath,
                FileMode.Create
            );

            await PdfFile!.CopyToAsync(stream);

            Song.PdfFileName = PdfFile.FileName;
        }
        else
        {
            var tempPath = Path.Combine(
                _environment.ContentRootPath,
                "Storage",
                "Temp",
                PendingPdfFileName!
            );

            if (!System.IO.File.Exists(tempPath))
            {
                ModelState.AddModelError(
                    nameof(PdfFile),
                    "The temporary PDF could not be found. Please select the PDF again."
                );

                return Page();
            }

            System.IO.File.Move(
                tempPath,
                storedFilePath
            );

            Song.PdfFileName =
                OriginalPdfFileName ?? "music-sheet.pdf";
        }

        Song.PdfPath = Path.Combine(
            "Storage",
            "Songs",
            storedFileName
        );

        Song.CreatedDate = DateTime.UtcNow;
        Song.UpdatedDate = DateTime.UtcNow;
        Song.IsActive = true;

        _context.Songs.Add(Song);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}