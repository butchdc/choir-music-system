using choir_music_system.Data;
using choir_music_system.Models;
using choir_music_system.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.MusicLibrary;

public class IndexModel : PageModel
{
    private readonly ChoirDbContext _context;
    private readonly PowerPointService _powerPointService;

    public IndexModel(
        ChoirDbContext context,
        PowerPointService powerPointService)
    {
        _context = context;
        _powerPointService = powerPointService;
    }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Filter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Part { get; set; }

    public IList<Song> Songs { get; set; }
        = new List<Song>();

    public string? ActivePart { get; set; }

    public string? ActiveFilter { get; set; }

    public async Task OnGetAsync()
    {
        ActivePart = Part;
        ActiveFilter = Filter;

        var query = _context.Songs
            .Where(x => x.IsActive)
            .AsQueryable();

        /*
         * MASS PART FILTER
         */

        if (!string.IsNullOrWhiteSpace(Part))
        {
            if (Part == "Not specified")
            {
                query = query.Where(x =>
                    x.SuggestedMassPart == null ||
                    x.SuggestedMassPart == "");
            }
            else
            {
                query = query.Where(x =>
                    x.SuggestedMassPart == Part);
            }
        }

        /*
         * DASHBOARD READINESS FILTERS
         */

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            switch (Filter.ToLowerInvariant())
            {
                case "lyrics":
                    query = query.Where(x =>
                        x.PresentationLyrics != null &&
                        x.PresentationLyrics != "");
                    break;

                case "missing-lyrics":
                    query = query.Where(x =>
                        x.PresentationLyrics == null ||
                        x.PresentationLyrics == "");
                    break;

                case "pdf":
                    query = query.Where(x =>
                        x.PdfPath != null &&
                        x.PdfPath != "");
                    break;

                case "missing-pdf":
                    query = query.Where(x =>
                        x.PdfPath == null ||
                        x.PdfPath == "");
                    break;

                case "one-license":
                    query = query.Where(x =>
                        x.OneLicenseNumber != null &&
                        x.OneLicenseNumber != "");
                    break;

                case "missing-one-license":
                    query = query.Where(x =>
                        x.OneLicenseNumber == null ||
                        x.OneLicenseNumber == "");
                    break;

                case "composer":
                    query = query.Where(x =>
                        x.Composer != null &&
                        x.Composer != "");
                    break;

                case "missing-composer":
                    query = query.Where(x =>
                        x.Composer == null ||
                        x.Composer == "");
                    break;

                case "needs-attention":
                    query = query.Where(x =>
                        x.PresentationLyrics == null ||
                        x.PresentationLyrics == "" ||
                        x.PdfPath == null ||
                        x.PdfPath == "" ||
                        x.OneLicenseNumber == null ||
                        x.OneLicenseNumber == "" ||
                        x.Composer == null ||
                        x.Composer == "");
                    break;
            }
        }

        /*
         * SEARCH
         */

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();

            query = query.Where(x =>
                x.Title.Contains(search) ||
                (x.Composer != null &&
                 x.Composer.Contains(search)) ||
                (x.OneLicenseNumber != null &&
                 x.OneLicenseNumber.Contains(search)) ||
                (x.SuggestedMassPart != null &&
                 x.SuggestedMassPart.Contains(search)));
        }

        Songs = await query
            .OrderBy(x => x.Title)
            .ToListAsync();
    }

    public async Task<IActionResult> OnGetGeneratePptAsync(int id)
    {
        var song = await _context.Songs
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (song is null)
        {
            return NotFound();
        }

        var filePath =
            _powerPointService.GenerateSongPresentation(song);

        var bytes =
            await System.IO.File.ReadAllBytesAsync(filePath);

        var safeFileName = string.Join(
            "_",
            song.Title.Split(Path.GetInvalidFileNameChars()));

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            $"{safeFileName}.pptx");
    }
}