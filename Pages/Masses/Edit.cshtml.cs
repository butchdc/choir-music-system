using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace choir_music_system.Pages.Masses;

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
    public Mass Mass { get; set; } = null!;

    [BindProperty]
    public IFormFile? BackgroundImage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var mass = await _context.Masses.FindAsync(id);

        if (mass is null)
            return NotFound();

        Mass = mass;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var existing = await _context.Masses.FindAsync(Mass.Id);

        if (existing is null)
            return NotFound();

        if (BackgroundImage is not null &&
            BackgroundImage.Length > 0)
        {
            var extension =
                Path.GetExtension(BackgroundImage.FileName);

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

            const long maxFileSize =
                10 * 1024 * 1024;

            if (BackgroundImage.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(BackgroundImage),
                    "The background image must be 10 MB or smaller.");
            }
        }

        if (!ModelState.IsValid)
        {
            // Preserve existing background information
            // when redisplaying the form.
            Mass.PresentationBackgroundPath =
                existing.PresentationBackgroundPath;

            return Page();
        }

        if (BackgroundImage is not null &&
            BackgroundImage.Length > 0)
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
                new FileStream(
                    fullPath,
                    FileMode.Create);

            await BackgroundImage.CopyToAsync(stream);

            existing.PresentationBackgroundPath =
                Path.Combine(
                    "Storage",
                    "Backgrounds",
                    storedFileName);
        }

        existing.Name = Mass.Name;
        existing.MassDate = Mass.MassDate;
        existing.Venue = Mass.Venue;
        existing.MassIntroduction = Mass.MassIntroduction;
        existing.Notes = Mass.Notes;
        existing.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}