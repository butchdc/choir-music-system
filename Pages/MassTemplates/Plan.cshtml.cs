using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.MassTemplates;

public class PlanModel : PageModel
{
    private readonly ChoirDbContext _context;

    public PlanModel(ChoirDbContext context)
    {
        _context = context;
    }

    public MassTemplate Template { get; set; } = null!;

    public IList<Song> Songs { get; set; }
        = new List<Song>();

    public IList<PresentationItem> PresentationLibrary { get; set; }
        = new List<PresentationItem>();

    public IList<MassTemplateItem> PlanItems { get; set; }
        = new List<MassTemplateItem>();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var template = await _context.MassTemplates
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (template is null)
        {
            return NotFound();
        }

        Template = template;

        Songs = await _context.Songs
            .Where(x => x.IsActive)
            .OrderBy(x => x.Title)
            .ToListAsync();

        PresentationLibrary =
            await _context.PresentationItems
                .Where(x => x.IsActive)
                .OrderBy(x => x.Type)
                .ThenBy(x => x.Language)
                .ThenBy(x => x.Title)
                .ToListAsync();

        PlanItems =
            await _context.MassTemplateItems
                .Where(x => x.MassTemplateId == id)
                .Include(x => x.Song)
                .Include(x => x.PresentationItem)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAddSongAsync(
        int id,
        int songId,
        string? massPart)
    {
        var exists = await _context.Songs
            .AnyAsync(x =>
                x.Id == songId &&
                x.IsActive);

        if (!exists)
        {
            return NotFound();
        }

        var nextOrder =
            await GetNextOrderAsync(id);

        _context.MassTemplateItems.Add(
            new MassTemplateItem
            {
                MassTemplateId = id,
                ItemType = "Song",
                SongId = songId,
                MassPart = massPart,
                DisplayOrder = nextOrder
            });

        await _context.SaveChangesAsync();

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult>
        OnPostAddPresentationAsync(
            int id,
            int presentationItemId,
            string? massPart)
    {
        var exists =
            await _context.PresentationItems
                .AnyAsync(x =>
                    x.Id == presentationItemId &&
                    x.IsActive);

        if (!exists)
        {
            return NotFound();
        }

        var nextOrder =
            await GetNextOrderAsync(id);

        _context.MassTemplateItems.Add(
            new MassTemplateItem
            {
                MassTemplateId = id,
                ItemType = "Presentation",
                PresentationItemId =
                    presentationItemId,
                MassPart = massPart,
                DisplayOrder = nextOrder
            });

        await _context.SaveChangesAsync();

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult>
        OnPostRemoveAsync(
            int id,
            int itemId)
    {
        var item =
            await _context.MassTemplateItems
                .FirstOrDefaultAsync(x =>
                    x.Id == itemId &&
                    x.MassTemplateId == id);

        if (item is not null)
        {
            _context.MassTemplateItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult>
        OnPostSaveOrderAsync(
            int id,
            List<int> itemIds)
    {
        var items =
            await _context.MassTemplateItems
                .Where(x =>
                    x.MassTemplateId == id)
                .ToListAsync();

        var lookup =
            items.ToDictionary(x => x.Id);

        var order = 10;

        foreach (var itemId in itemIds)
        {
            if (lookup.TryGetValue(
                itemId,
                out var item))
            {
                item.DisplayOrder = order;
                order += 10;
            }
        }

        await _context.SaveChangesAsync();

        return new JsonResult(new
        {
            success = true
        });
    }

    private async Task<int> GetNextOrderAsync(
        int templateId)
    {
        var max =
            await _context.MassTemplateItems
                .Where(x =>
                    x.MassTemplateId ==
                    templateId)
                .Select(x =>
                    (int?)x.DisplayOrder)
                .MaxAsync() ?? 0;

        return max + 10;
    }
}