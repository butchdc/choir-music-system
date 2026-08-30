using choir_music_system.Data;
using choir_music_system.Models;
using choir_music_system.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.MusicLibrary;

public class BulkUploadModel : PageModel
{
    private readonly ChoirDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly PdfMetadataExtractorService _metadataExtractor;

    public BulkUploadModel(
        ChoirDbContext context,
        IWebHostEnvironment environment,
        PdfMetadataExtractorService metadataExtractor)
    {
        _context = context;
        _environment = environment;
        _metadataExtractor = metadataExtractor;
    }

    [BindProperty]
    public List<IFormFile> PdfFiles { get; set; } = new();

    public List<string> Results { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (PdfFiles.Count == 0)
        {
            ModelState.AddModelError(
                nameof(PdfFiles),
                "Please select at least one PDF file."
            );

            return Page();
        }

        var storageFolder = Path.Combine(
            _environment.ContentRootPath,
            "Storage",
            "Songs"
        );

        Directory.CreateDirectory(storageFolder);

        const long maxFileSize = 25 * 1024 * 1024;

        foreach (var pdfFile in PdfFiles)
        {
            if (pdfFile.Length == 0)
            {
                Results.Add(
                    $"{pdfFile.FileName}: skipped because the file is empty."
                );

                continue;
            }

            if (pdfFile.Length > maxFileSize)
            {
                Results.Add(
                    $"{pdfFile.FileName}: skipped because it is larger than 25 MB."
                );

                continue;
            }

            var extension = Path.GetExtension(pdfFile.FileName);

            if (!string.Equals(
                    extension,
                    ".pdf",
                    StringComparison.OrdinalIgnoreCase))
            {
                Results.Add(
                    $"{pdfFile.FileName}: skipped because it is not a PDF."
                );

                continue;
            }

            var duplicateExists = await _context.Songs
                .AnyAsync(x =>
                    x.PdfFileName == pdfFile.FileName);

            if (duplicateExists)
            {
                Results.Add(
                    $"{pdfFile.FileName}: skipped because a file with this name already exists."
                );

                continue;
            }

            var storedFileName =
                $"{Guid.NewGuid():N}.pdf";

            var storedFilePath = Path.Combine(
                storageFolder,
                storedFileName
            );

            await using (var stream = new FileStream(
                storedFilePath,
                FileMode.Create))
            {
                await pdfFile.CopyToAsync(stream);
            }

            var metadata =
                _metadataExtractor.Extract(storedFilePath);

            var title =
                !string.IsNullOrWhiteSpace(metadata.Title)
                    ? metadata.Title
                    : Path.GetFileNameWithoutExtension(
                        pdfFile.FileName
                    );

            var Song = new Song
            {
                Title = title,
                PdfFileName = pdfFile.FileName,
                PdfPath = Path.Combine(
                    "Storage",
                    "Songs",
                    storedFileName
                ),
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Songs.Add(Song);

            Results.Add(
                $"{pdfFile.FileName}: uploaded as \"{title}\"."
            );
        }

        await _context.SaveChangesAsync();

        return Page();
    }
}