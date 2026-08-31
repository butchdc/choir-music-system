using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

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

    [BindProperty]
    public int? MassTemplateId { get; set; }

    public IList<MassTemplate> MassTemplates { get; set; }
        = new List<MassTemplate>();


    public async Task OnGetAsync()
    {
        await LoadTemplatesAsync();
    }


    public async Task<IActionResult> OnPostAsync()
    {
        if (BackgroundImage is not null &&
            BackgroundImage.Length > 0)
        {
            var extension =
                Path.GetExtension(
                    BackgroundImage.FileName
                );

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

            if (BackgroundImage.Length >
                maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(BackgroundImage),
                    "The background image must be 10 MB or smaller.");
            }
        }


        MassTemplate? selectedTemplate = null;

        if (MassTemplateId.HasValue)
        {
            selectedTemplate =
                await _context.MassTemplates
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x =>
                        x.Id == MassTemplateId.Value &&
                        x.IsActive);

            if (selectedTemplate is null)
            {
                ModelState.AddModelError(
                    nameof(MassTemplateId),
                    "The selected Mass template is unavailable.");
            }
        }


        if (!ModelState.IsValid)
        {
            await LoadTemplatesAsync();

            return Page();
        }


        if (BackgroundImage is not null &&
            BackgroundImage.Length > 0)
        {
            var extension =
                Path.GetExtension(
                    BackgroundImage.FileName
                )
                .ToLowerInvariant();

            var backgroundFolder =
                Path.Combine(
                    _environment.ContentRootPath,
                    "Storage",
                    "Backgrounds"
                );

            Directory.CreateDirectory(
                backgroundFolder
            );

            var storedFileName =
                $"{Guid.NewGuid():N}{extension}";

            var fullPath =
                Path.Combine(
                    backgroundFolder,
                    storedFileName
                );

            await using var stream =
                new FileStream(
                    fullPath,
                    FileMode.Create
                );

            await BackgroundImage
                .CopyToAsync(stream);

            Mass.PresentationBackgroundPath =
                Path.Combine(
                    "Storage",
                    "Backgrounds",
                    storedFileName
                );
        }


        Mass.CreatedDate = DateTime.UtcNow;
        Mass.UpdatedDate = DateTime.UtcNow;

        _context.Masses.Add(Mass);

        await _context.SaveChangesAsync();


        /*
         * COPY TEMPLATE PLAN
         */

        if (selectedTemplate is not null)
        {
            var templateItems =
                selectedTemplate.Items
                    .OrderBy(x => x.DisplayOrder)
                    .ToList();

            foreach (var templateItem in templateItems)
            {
                var planItem =
                    new MassPlanItem
                    {
                        MassId = Mass.Id,

                        ItemType =
                            templateItem.ItemType,

                        SongId =
                            templateItem.SongId,

                        PresentationItemId =
                            templateItem.PresentationItemId,

                        MassPart =
                            templateItem.MassPart,

                        DisplayOrder =
                            templateItem.DisplayOrder
                    };

                _context.MassPlanItems.Add(
                    planItem
                );
            }


            /*
             * Maintain the existing MassSongs
             * collection for the PDF Music Plan.
             */

            var templateSongs =
                templateItems
                    .Where(x =>
                        x.ItemType == "Song" &&
                        x.SongId.HasValue)
                    .ToList();

            foreach (var templateSong in templateSongs)
            {
                _context.MassSongs.Add(
                    new MassSong
                    {
                        MassId = Mass.Id,

                        SongId =
                            templateSong.SongId!.Value,

                        MassPart =
                            templateSong.MassPart ??
                            string.Empty,

                        DisplayOrder =
                            templateSong.DisplayOrder
                    }
                );
            }


            await _context.SaveChangesAsync();
        }


        return RedirectToPage(
            "Plan",
            new
            {
                id = Mass.Id
            }
        );
    }


    private async Task LoadTemplatesAsync()
    {
        MassTemplates =
            await _context.MassTemplates
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();
    }
}