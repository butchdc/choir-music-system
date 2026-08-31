using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages;

public class IndexModel : PageModel
{
    private readonly ChoirDbContext _context;

    public IndexModel(ChoirDbContext context)
    {
        _context = context;
    }

    public int SongCount { get; set; }

    public int UpcomingMassCount { get; set; }

    public int MassTemplateCount { get; set; }

    public int NeedsAttentionCount { get; set; }

    public int SongsWithLyricsCount { get; set; }

    public int SongsMissingLyricsCount { get; set; }

    public int SongsWithPdfCount { get; set; }

    public int SongsMissingPdfCount { get; set; }

    public int SongsWithOneLicenseCount { get; set; }

    public int SongsMissingOneLicenseCount { get; set; }

    public int SongsWithComposerCount { get; set; }

    public int SongsMissingComposerCount { get; set; }

    public UpcomingMassItem? NextMass { get; set; }

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


        UpcomingMassCount = await _context.Masses
            .CountAsync(x => x.MassDate >= today);


        MassTemplateCount = await _context.MassTemplates
            .CountAsync(x => x.IsActive);


        SongsWithLyricsCount = await _context.Songs
            .CountAsync(x =>
                x.IsActive &&
                x.PresentationLyrics != null &&
                x.PresentationLyrics != "");

        SongsMissingLyricsCount =
            SongCount - SongsWithLyricsCount;


        SongsWithPdfCount = await _context.Songs
            .CountAsync(x =>
                x.IsActive &&
                x.PdfPath != null &&
                x.PdfPath != "");

        SongsMissingPdfCount =
            SongCount - SongsWithPdfCount;


        SongsWithOneLicenseCount = await _context.Songs
            .CountAsync(x =>
                x.IsActive &&
                x.OneLicenseNumber != null &&
                x.OneLicenseNumber != "");

        SongsMissingOneLicenseCount =
            SongCount - SongsWithOneLicenseCount;


        SongsWithComposerCount = await _context.Songs
            .CountAsync(x =>
                x.IsActive &&
                x.Composer != null &&
                x.Composer != "");

        SongsMissingComposerCount =
            SongCount - SongsWithComposerCount;


        NeedsAttentionCount = await _context.Songs
            .CountAsync(x =>
                x.IsActive &&
                (
                    x.PresentationLyrics == null ||
                    x.PresentationLyrics == "" ||

                    x.PdfPath == null ||
                    x.PdfPath == "" ||

                    x.OneLicenseNumber == null ||
                    x.OneLicenseNumber == "" ||

                    x.Composer == null ||
                    x.Composer == ""
                ));


        UpcomingMasses = await _context.Masses
            .Where(x => x.MassDate >= today)
            .OrderBy(x => x.MassDate)
            .ThenBy(x => x.Name)
            .Take(5)
            .Select(x => new UpcomingMassItem
            {
                Id = x.Id,

                Name = x.Name,

                MassDate = x.MassDate,

                MusicCount = _context.MassSongs
                    .Count(m => m.MassId == x.Id),

                PresentationItemCount =
                    _context.MassPlanItems
                        .Count(p =>
                            p.MassId == x.Id &&
                            p.ItemType == "Presentation"),

                HasBackground =
                    x.PresentationBackgroundPath != null &&
                    x.PresentationBackgroundPath != ""
            })
            .ToListAsync();


        NextMass = UpcomingMasses.FirstOrDefault();


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
    }


    public class UpcomingMassItem
    {
        public int Id { get; set; }

        public string Name { get; set; }
            = string.Empty;

        public DateTime MassDate { get; set; }

        public int MusicCount { get; set; }

        public int PresentationItemCount { get; set; }

        public bool HasBackground { get; set; }
    }


    public class MassPartCount
    {
        public string Name { get; set; }
            = string.Empty;

        public int Count { get; set; }
    }
}