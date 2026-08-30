using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace choir_music_system.Pages.Masses;

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
    public Mass Mass { get; set; } = new()
    {
        MassDate = DateTime.Today
    };

    [BindProperty]
    public IFormFile? BackgroundImage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (BackgroundImage is not null && BackgroundImage.Length > 0)
        {
            var extension = Path.GetExtension(BackgroundImage.FileName);

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png"
            };

            if (!allowedExtensions.Contains(
                extension,
                StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(BackgroundImage),
                    "Only JPG and PNG background images are allowed.");
            }

            const long maxFileSize = 10 * 1024 * 1024;

            if (BackgroundImage.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(BackgroundImage),
                    "The background image must be 10 MB or smaller.");
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (BackgroundImage is not null && BackgroundImage.Length > 0)
        {
            var extension =
                Path.GetExtension(BackgroundImage.FileName)
                    .ToLowerInvariant();

            var backgroundFolder = Path.Combine(
                _environment.ContentRootPath,
                "Storage",
                "Backgrounds");

            Directory.CreateDirectory(backgroundFolder);

            var storedFileName =
                $"{Guid.NewGuid():N}{extension}";

            var fullPath = Path.Combine(
                backgroundFolder,
                storedFileName);

            await using var stream =
                new FileStream(fullPath, FileMode.Create);

            await BackgroundImage.CopyToAsync(stream);

            Mass.PresentationBackgroundPath = Path.Combine(
                "Storage",
                "Backgrounds",
                storedFileName);
        }

        Mass.CreatedDate = DateTime.UtcNow;
        Mass.UpdatedDate = DateTime.UtcNow;

        _context.Masses.Add(Mass);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}