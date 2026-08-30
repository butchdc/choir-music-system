using choir_music_system.Data;
using choir_music_system.Models;
using choir_music_system.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.Masses;

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

    public IList<Mass> Masses { get; set; }
        = new List<Mass>();

    public string? ActiveFilter { get; set; }

    public async Task<IActionResult> OnGetGeneratePptAsync(int id)
    {
        var mass = await _context.Masses
            .Include(x => x.PlanItems)
                .ThenInclude(x => x.Song)
            .Include(x => x.PlanItems)
                .ThenInclude(x => x.PresentationItem)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (mass is null)
        {
            return NotFound();
        }

        var filePath =
            _powerPointService.GenerateMassPresentation(mass);

        var bytes =
            await System.IO.File.ReadAllBytesAsync(filePath);

        var safeName = string.Join(
            "_",
            mass.Name.Split(Path.GetInvalidFileNameChars()));

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            $"{safeName}-{mass.MassDate:yyyyMMdd}.pptx");
    }

    public async Task OnGetAsync(string? filter)
    {
        ActiveFilter = filter;

        var today = DateTime.Today;

        var query = _context.Masses
            .Include(x => x.Songs)
                .ThenInclude(x => x.Song)
            .AsQueryable();

        switch (filter?.ToLowerInvariant())
        {
            case "upcoming":
                query = query.Where(x => x.MassDate >= today);
                break;

            case "past":
                query = query.Where(x => x.MassDate < today);
                break;
        }

        Masses = await query
            .OrderByDescending(x => x.MassDate)
            .ToListAsync();
    }

    public async Task<IActionResult> OnGetDuplicateAsync(int id)
    {
        var sourceMass = await _context.Masses
            .FirstOrDefaultAsync(x => x.Id == id);

        if (sourceMass is null)
        {
            return NotFound();
        }

        var sourceSelections = await _context.MassSongs
            .Where(x => x.MassId == id)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        var sourcePlanItems = await _context.MassPlanItems
            .Where(x => x.MassId == id)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        var newMass = new Mass
        {
            Name = $"{sourceMass.Name} Copy",
            MassDate = sourceMass.MassDate.AddDays(7),
            Notes = sourceMass.Notes,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        _context.Masses.Add(newMass);

        await _context.SaveChangesAsync();

        foreach (var selection in sourceSelections)
        {
            _context.MassSongs.Add(
                new MassSong
                {
                    MassId = newMass.Id,
                    SongId = selection.SongId,
                    MassPart = selection.MassPart,
                    DisplayOrder = selection.DisplayOrder
                }
            );
        }

        foreach (var item in sourcePlanItems)
        {
            _context.MassPlanItems.Add(
                new MassPlanItem
                {
                    MassId = newMass.Id,
                    ItemType = item.ItemType,
                    SongId = item.SongId,
                    PresentationItemId = item.PresentationItemId,
                    MassPart = item.MassPart,
                    DisplayOrder = item.DisplayOrder
                }
            );
        }

        await _context.SaveChangesAsync();

        return RedirectToPage(
            "Edit",
            new { id = newMass.Id }
        );
    }
}