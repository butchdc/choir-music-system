using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace choir_music_system.Pages.MusicLibrary;

public class CreateModel : PageModel
{
    private readonly ChoirDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public CreateModel(
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

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (PdfFile is null || PdfFile.Length == 0)
        {
            ModelState.AddModelError(
                nameof(PdfFile),
                "Please select a PDF file."
            );
        }

        if (PdfFile is not null)
        {

            const long maxFileSize = 25 * 1024 * 1024;

            if (PdfFile.Length > maxFileSize)
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
            "MusicSheets"
        );

        Directory.CreateDirectory(storageFolder);

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
            await PdfFile!.CopyToAsync(stream);
        }

        MusicSheet.PdfFileName = PdfFile!.FileName;
        MusicSheet.PdfPath = Path.Combine(
            "Storage",
            "MusicSheets",
            storedFileName
        );

        MusicSheet.CreatedDate = DateTime.UtcNow;
        MusicSheet.UpdatedDate = DateTime.UtcNow;
        MusicSheet.IsActive = true;

        _context.MusicSheets.Add(MusicSheet);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}