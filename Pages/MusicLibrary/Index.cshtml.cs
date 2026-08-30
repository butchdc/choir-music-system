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

    public IList<Song> Songs { get; set; }
        = new List<Song>();

    public string? ActivePart { get; set; }

    public async Task OnGetAsync(string? part)
    {
        ActivePart = part;

        var query = _context.Songs
            .Where(x => x.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(part))
        {
            if (part == "Not specified")
            {
                query = query.Where(x =>
                    x.SuggestedMassPart == null ||
                    x.SuggestedMassPart == "");
            }
            else
            {
                query = query.Where(x =>
                    x.SuggestedMassPart == part);
            }
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