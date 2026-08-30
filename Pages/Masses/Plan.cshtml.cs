using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.Masses;

public class PlanModel : PageModel
{
    private readonly ChoirDbContext _context;

    public PlanModel(ChoirDbContext context)
    {
        _context = context;
    }

    public Mass Mass { get; set; } = null!;

    public IList<Song> Songs { get; set; }
        = new List<Song>();

    [BindProperty]
    public List<MassPartSelection> Selections { get; set; }
        = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var mass = await _context.Masses
            .FirstOrDefaultAsync(x => x.Id == id);

        if (mass is null)
        {
            return NotFound();
        }

        Mass = mass;

        await EnsureSongPlanItemsAsync(id);

        Songs = await _context.Songs
            .Where(x => x.IsActive)
            .OrderBy(x => x.Title)
            .ToListAsync();

        PresentationLibrary = await _context.PresentationItems
            .Where(x => x.IsActive)
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Title)
            .ToListAsync();

        PlanItems = await _context.MassPlanItems
            .Where(x => x.MassId == id)
            .Include(x => x.Song)
            .Include(x => x.PresentationItem)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        var existingSelections = await _context.MassSongs
            .Where(x => x.MassId == id)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        Selections = new List<MassPartSelection>();

        var displayOrder = 1;

        foreach (var part in GetMassParts())
        {
            var partSelections = existingSelections
                .Where(x => x.MassPart == part)
                .OrderBy(x => x.DisplayOrder)
                .ToList();

            if (partSelections.Count == 0)
            {
                Selections.Add(new MassPartSelection
                {
                    MassPart = part,
                    SongId = null,
                    DisplayOrder = displayOrder++
                });

                continue;
            }

            foreach (var existing in partSelections)
            {
                Selections.Add(new MassPartSelection
                {
                    MassPart = part,
                    SongId = existing.SongId,
                    DisplayOrder = displayOrder++
                });
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var mass = await _context.Masses
            .FirstOrDefaultAsync(x => x.Id == id);

        if (mass is null)
        {
            return NotFound();
        }

        var songSelections = Selections
            .Where(x => x.SongId.HasValue)
            .ToList();

        // Keep MassSongs in sync for Music Pack / existing workflows.
        var existingMassSongs = await _context.MassSongs
            .Where(x => x.MassId == id)
            .ToListAsync();

        _context.MassSongs.RemoveRange(existingMassSongs);

        var massSongOrder = 1;

        foreach (var selection in songSelections)
        {
            _context.MassSongs.Add(new MassSong
            {
                MassId = id,
                SongId = selection.SongId!.Value,
                MassPart = selection.MassPart,
                DisplayOrder = massSongOrder++
            });
        }

        // Load the current unified presentation plan.
        var existingPlanItems = await _context.MassPlanItems
            .Where(x => x.MassId == id)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        var existingSongItems = existingPlanItems
            .Where(x => x.ItemType == "Song")
            .ToList();

        // Track which existing song rows have been reused.
        var usedSongItemIds = new HashSet<int>();

        foreach (var selection in songSelections)
        {
            // Try to reuse an existing matching Song plan item first.
            var existingItem = existingSongItems
                .FirstOrDefault(x =>
                    !usedSongItemIds.Contains(x.Id) &&
                    x.SongId == selection.SongId &&
                    string.Equals(
                        x.MassPart,
                        selection.MassPart,
                        StringComparison.OrdinalIgnoreCase));

            if (existingItem is not null)
            {
                usedSongItemIds.Add(existingItem.Id);
                continue;
            }

            // This is a newly added song.
            // Put it at the end of the current presentation order.
            var nextOrder = existingPlanItems.Count == 0
                ? 10
                : existingPlanItems.Max(x => x.DisplayOrder) + 10;

            var newItem = new MassPlanItem
            {
                MassId = id,
                ItemType = "Song",
                SongId = selection.SongId!.Value,
                MassPart = selection.MassPart,
                DisplayOrder = nextOrder
            };

            _context.MassPlanItems.Add(newItem);

            existingPlanItems.Add(newItem);
        }

        // Remove Song plan items that are no longer selected.
        var removedSongItems = existingSongItems
            .Where(x => !usedSongItemIds.Contains(x.Id))
            .ToList();

        _context.MassPlanItems.RemoveRange(removedSongItems);

        mass.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToPage(new { id });
    }
    public async Task<IActionResult> OnPostAddPresentationAsync(
        int id,
        int presentationItemId,
        string? massPart)
    {
        var itemExists = await _context.PresentationItems
            .AnyAsync(x =>
                x.Id == presentationItemId &&
                x.IsActive);

        if (!itemExists)
        {
            return NotFound();
        }

        var nextOrder = await _context.MassPlanItems
            .Where(x => x.MassId == id)
            .Select(x => (int?)x.DisplayOrder)
            .MaxAsync() ?? 0;

        _context.MassPlanItems.Add(new MassPlanItem
        {
            MassId = id,
            ItemType = "Presentation",
            PresentationItemId = presentationItemId,
            MassPart = massPart,
            DisplayOrder = nextOrder + 1
        });

        await _context.SaveChangesAsync();

        return RedirectToPage(new { id });
    }

    private static List<string> GetMassParts()
    {
        return new List<string>
    {
        "Entrance",
        "Kyrie",
        "Gloria",
        "Psalm",
        "Alleluia",
        "Offertory",
        "Holy",
        "Memorial Acclamation",
        "Amen",
        "Our Father",
        "Lamb of God",
        "Communion",
        "Recessional"
    };
    }

    public async Task<IActionResult> OnPostRemovePlanItemAsync(
    int id,
    int planItemId)
    {
        var item = await _context.MassPlanItems
            .FirstOrDefaultAsync(x =>
                x.Id == planItemId &&
                x.MassId == id);

        if (item is not null)
        {
            _context.MassPlanItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage(new { id });
    }

    public class MassPartSelection
    {
        public string MassPart { get; set; } = string.Empty;

        public int? SongId { get; set; }

        public int DisplayOrder { get; set; }
    }

    public IList<PresentationItem> PresentationLibrary { get; set; }
    = new List<PresentationItem>();

    public IList<MassPlanItem> PlanItems { get; set; }
        = new List<MassPlanItem>();

    private async Task EnsureSongPlanItemsAsync(int massId)
    {
        var alreadyMigrated = await _context.MassPlanItems
            .AnyAsync(x =>
                x.MassId == massId &&
                x.ItemType == "Song");

        if (alreadyMigrated)
        {
            return;
        }

        var songs = await _context.MassSongs
            .Where(x => x.MassId == massId)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        if (songs.Count == 0)
        {
            return;
        }

        var order = 10;

        foreach (var song in songs)
        {
            _context.MassPlanItems.Add(new MassPlanItem
            {
                MassId = massId,
                ItemType = "Song",
                SongId = song.SongId,
                MassPart = song.MassPart,
                DisplayOrder = order
            });

            order += 10;
        }

        // Any presentation items that were already added
        // are temporarily placed after the existing songs.
        var presentationItems = await _context.MassPlanItems
            .Where(x =>
                x.MassId == massId &&
                x.ItemType == "Presentation")
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        foreach (var item in presentationItems)
        {
            item.DisplayOrder = order;
            order += 10;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IActionResult> OnPostSavePlanOrderAsync(
    int id,
    List<int> itemIds)
    {
        var items = await _context.MassPlanItems
            .Where(x => x.MassId == id)
            .ToListAsync();

        var lookup = items.ToDictionary(x => x.Id);

        var displayOrder = 10;

        foreach (var itemId in itemIds)
        {
            if (lookup.TryGetValue(itemId, out var item))
            {
                item.DisplayOrder = displayOrder;
                displayOrder += 10;
            }
        }

        await _context.SaveChangesAsync();

        return new JsonResult(new
        {
            success = true
        });
    }
}