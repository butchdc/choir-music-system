using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages;

public class IndexModel : PageModel
{
    private readonly ChoirDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public IndexModel(
        ChoirDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public int SongCount { get; set; }
    public int MassCount { get; set; }
    public int UpcomingMassCount { get; set; }
    public int MusicPackCount { get; set; }

    public IList<UpcomingMassItem> UpcomingMasses { get; set; }
        = new List<UpcomingMassItem>();

    public IList<Song> RecentMusic { get; set; }
        = new List<Song>();

    public IList<MassPartCount> LibraryBreakdown { get; set; }
        = new List<MassPartCount>();

    public async Task OnGetAsync()
    {
        var today = DateTime.Today;

        SongCount = await _context.Songs
            .CountAsync(x => x.IsActive);

        MassCount = await _context.Masses.CountAsync();

        UpcomingMassCount = await _context.Masses
            .CountAsync(x => x.MassDate >= today);

        UpcomingMasses = await _context.Masses
            .Where(x => x.MassDate >= today)
            .OrderBy(x => x.MassDate)
            .Take(5)
            .Select(x => new UpcomingMassItem
            {
                Id = x.Id,
                Name = x.Name,
                MassDate = x.MassDate,

                MusicCount = _context.MassSongs
                    .Count(m => m.MassId == x.Id)
            })
            .ToListAsync();

        RecentMusic = await _context.Songs
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.UpdatedDate)
            .Take(5)
            .ToListAsync();

        LibraryBreakdown = await _context.Songs
            .Where(x => x.IsActive)
            .GroupBy(x =>
                string.IsNullOrWhiteSpace(x.SuggestedMassPart)
                    ? "Not specified"
                    : x.SuggestedMassPart)
            .Select(x => new MassPartCount
            {
                Name = x.Key,
                Count = x.Count()
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Name)
            .ToListAsync();

        var musicPackFolder = Path.Combine(
            _environment.ContentRootPath,
            "Storage",
            "Generated",
            "MusicPacks");

        if (Directory.Exists(musicPackFolder))
        {
            MusicPackCount = Directory
                .GetFiles(musicPackFolder, "*.pdf")
                .Length;
        }
    }

    public class UpcomingMassItem
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime MassDate { get; set; }

        public int MusicCount { get; set; }
    }

    public class MassPartCount
    {
        public string Name { get; set; } = string.Empty;

        public int Count { get; set; }
    }
}