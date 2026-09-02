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

    public IList<PresentationItem> PresentationLibrary { get; set; }
        = new List<PresentationItem>();

    public IList<MassPlanItem> PlanItems { get; set; }
        = new List<MassPlanItem>();

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
        await EnsureMassTitlePlanItemAsync(id);

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

        // ---------------------------------------------------------
        // Keep MassSongs for Music Pack / existing workflows.
        // ---------------------------------------------------------

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

        // ---------------------------------------------------------
        // Keep the existing Presentation Order.
        //
        // Saving Mass Parts must not rebuild MassPlanItems because
        // the user may have manually arranged:
        //
        // HC Safety
        // Prelude Song
        // Mass Title
        // Entrance
        //
        // Existing plan items keep their DisplayOrder. We only
        // reconcile songs that were added or removed.
        // ---------------------------------------------------------

        var existingPlanItems = await _context.MassPlanItems
            .Where(x => x.MassId == id)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        var existingSongPlanItems = existingPlanItems
            .Where(x =>
                string.Equals(
                    x.ItemType,
                    "Song",
                    StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.DisplayOrder)
            .ToList();

        // ---------------------------------------------------------
        // Match the selected songs against existing Song plan items.
        //
        // Matching includes SongId + MassPart and supports the same
        // song appearing more than once.
        // ---------------------------------------------------------

        var unmatchedExistingSongs =
            new List<MassPlanItem>(existingSongPlanItems);

        var newSongSelections =
            new List<MassPartSelection>();

        foreach (var selection in songSelections)
        {
            var existingItem =
                unmatchedExistingSongs.FirstOrDefault(x =>
                    x.SongId == selection.SongId &&
                    string.Equals(
                        x.MassPart,
                        selection.MassPart,
                        StringComparison.OrdinalIgnoreCase));

            if (existingItem is not null)
            {
                // Song already exists in Presentation Order.
                // Keep its exact DisplayOrder.
                unmatchedExistingSongs.Remove(existingItem);
            }
            else
            {
                // Newly selected song.
                newSongSelections.Add(selection);
            }
        }

        // ---------------------------------------------------------
        // Remove songs that are no longer selected.
        // ---------------------------------------------------------

        if (unmatchedExistingSongs.Count > 0)
        {
            _context.MassPlanItems.RemoveRange(
                unmatchedExistingSongs);
        }

        // ---------------------------------------------------------
        // Insert newly selected songs into the Presentation Order.
        //
        // Existing items keep their relative order. A new song is
        // placed after the last song for the same Mass Part.
        //
        // If that Mass Part does not yet exist, place the song
        // before the first song belonging to a later Mass Part.
        // ---------------------------------------------------------

        var orderedPlanItems = existingPlanItems
            .Where(x => !unmatchedExistingSongs.Contains(x))
            .OrderBy(x => x.DisplayOrder)
            .ToList();

        var massParts = GetMassParts();

        foreach (var selection in newSongSelections)
        {
            var newItem = new MassPlanItem
            {
                MassId = id,
                ItemType = "Song",
                SongId = selection.SongId!.Value,
                MassPart = selection.MassPart
            };

            // -----------------------------------------------------
            // First preference:
            // after the last existing song for the same Mass Part.
            // -----------------------------------------------------

            var lastSamePartIndex =
                orderedPlanItems.FindLastIndex(x =>
                    string.Equals(
                        x.ItemType,
                        "Song",
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        x.MassPart,
                        selection.MassPart,
                        StringComparison.OrdinalIgnoreCase));

            if (lastSamePartIndex >= 0)
            {
                orderedPlanItems.Insert(
                    lastSamePartIndex + 1,
                    newItem);
            }
            else
            {
                // -------------------------------------------------
                // No song for this Mass Part exists yet.
                //
                // Find the first song belonging to a later
                // Mass Part and insert immediately before it.
                // -------------------------------------------------

                var selectedPartIndex =
                    massParts.FindIndex(x =>
                        string.Equals(
                            x,
                            selection.MassPart,
                            StringComparison.OrdinalIgnoreCase));

                var insertIndex = -1;

                if (selectedPartIndex >= 0)
                {
                    for (var i = 0;
                         i < orderedPlanItems.Count;
                         i++)
                    {
                        var existingItem =
                            orderedPlanItems[i];

                        if (!string.Equals(
                                existingItem.ItemType,
                                "Song",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var existingPartIndex =
                            massParts.FindIndex(x =>
                                string.Equals(
                                    x,
                                    existingItem.MassPart,
                                    StringComparison.OrdinalIgnoreCase));

                        if (existingPartIndex >
                            selectedPartIndex)
                        {
                            insertIndex = i;
                            break;
                        }
                    }
                }

                if (insertIndex >= 0)
                {
                    orderedPlanItems.Insert(
                        insertIndex,
                        newItem);
                }
                else
                {
                    // ---------------------------------------------
                    // No later Mass Part exists.
                    //
                    // Put the new song immediately after the final
                    // song instead of after Presentation/System
                    // items at the bottom.
                    // ---------------------------------------------

                    var lastSongIndex =
                        orderedPlanItems.FindLastIndex(x =>
                            string.Equals(
                                x.ItemType,
                                "Song",
                                StringComparison.OrdinalIgnoreCase));

                    if (lastSongIndex >= 0)
                    {
                        orderedPlanItems.Insert(
                            lastSongIndex + 1,
                            newItem);
                    }
                    else
                    {
                        orderedPlanItems.Add(
                            newItem);
                    }
                }
            }

            _context.MassPlanItems.Add(
                newItem);
        }


        // ---------------------------------------------------------
        // Renumber the Presentation Order.
        //
        // This does not change the relative order of the existing
        // items. It simply creates clean DisplayOrder gaps again.
        // ---------------------------------------------------------

        var nextPlanOrder = 10;

        foreach (var item in orderedPlanItems)
        {
            item.DisplayOrder =
                nextPlanOrder;

            nextPlanOrder += 10;
        }

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
            DisplayOrder = nextOrder + 10
        });

        await _context.SaveChangesAsync();

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRemovePlanItemAsync(
        int id,
        int planItemId)
    {
        var item = await _context.MassPlanItems
            .FirstOrDefaultAsync(x =>
                x.Id == planItemId &&
                x.MassId == id);

        if (item is not null &&
            !string.Equals(
                item.ItemType,
                "MassTitle",
                StringComparison.OrdinalIgnoreCase))
        {
            _context.MassPlanItems.Remove(item);

            await _context.SaveChangesAsync();
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostSavePlanOrderAsync(
        int id,
        [FromForm] List<int> itemIds)
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

        var existingNonSongItems =
            await _context.MassPlanItems
                .Where(x =>
                    x.MassId == massId &&
                    x.ItemType != "Song")
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

        var order = 10;

        foreach (var song in songs)
        {
            _context.MassPlanItems.Add(
                new MassPlanItem
                {
                    MassId = massId,
                    ItemType = "Song",
                    SongId = song.SongId,
                    MassPart = song.MassPart,
                    DisplayOrder = order
                });

            order += 10;
        }

        foreach (var item in existingNonSongItems)
        {
            item.DisplayOrder = order;
            order += 10;
        }

        await _context.SaveChangesAsync();
    }

    private async Task EnsureMassTitlePlanItemAsync(int massId)
    {
        var existingTitle =
            await _context.MassPlanItems
                .FirstOrDefaultAsync(x =>
                    x.MassId == massId &&
                    x.ItemType == "MassTitle");

        if (existingTitle is not null)
        {
            return;
        }

        var existingItems =
            await _context.MassPlanItems
                .Where(x => x.MassId == massId)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

        // Existing Masses historically generated the title first.
        // Preserve that behaviour when introducing the new
        // draggable title item.
        foreach (var item in existingItems)
        {
            item.DisplayOrder += 10;
        }

        _context.MassPlanItems.Add(
            new MassPlanItem
            {
                MassId = massId,
                ItemType = "MassTitle",
                MassPart = null,
                DisplayOrder = 10
            });

        await _context.SaveChangesAsync();
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

    public class MassPartSelection
    {
        public string MassPart { get; set; }
            = string.Empty;

        public int? SongId { get; set; }

        public int DisplayOrder { get; set; }
    }
}