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

    private const int PageSize = 20;


    public IndexModel(
        ChoirDbContext context,
        PowerPointService powerPointService)
    {
        _context = context;
        _powerPointService = powerPointService;
    }


    public IList<Mass> Masses { get; set; }
        = new List<Mass>();


    public string ActiveFilter { get; set; }
        = "upcoming";


    public string? Search { get; set; }


    public int PageNumber { get; set; }
        = 1;


    public int TotalPages { get; set; }


    public int TotalCount { get; set; }


    public async Task<IActionResult> OnGetGeneratePptAsync(
        int id)
    {
        var mass = await _context.Masses

            .Include(x => x.PlanItems)
                .ThenInclude(x => x.Song)

            .Include(x => x.PlanItems)
                .ThenInclude(x => x.PresentationItem)

            .FirstOrDefaultAsync(
                x => x.Id == id
            );


        if (mass is null)
        {
            return NotFound();
        }


        var filePath =
            _powerPointService
                .GenerateMassPresentation(mass);


        var fileBytes =
            await System.IO.File
                .ReadAllBytesAsync(filePath);


        var safeVenue =
            MakeSafeFileName(
                string.IsNullOrWhiteSpace(mass.Venue)
                    ? "Venue"
                    : mass.Venue);

        var safeMassName =
            MakeSafeFileName(mass.Name);

        var downloadFileName =
            $"{safeVenue}-{safeMassName}-{mass.MassDate:yyyyMMdd}.pptx";

        System.IO.File.Delete(filePath);

        return File(
            fileBytes,
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            downloadFileName);
    }


    public async Task<IActionResult>
        OnGetSaveAsTemplateAsync(
            int id)
    {
        var mass = await _context.Masses

            .Include(x => x.PlanItems)

            .FirstOrDefaultAsync(
                x => x.Id == id
            );


        if (mass is null)
        {
            return NotFound();
        }


        var template =
            new MassTemplate
            {
                Name =
                    $"{mass.Name} Template",

                Notes =
                    mass.Notes,

                IsActive =
                    true,

                CreatedDate =
                    DateTime.UtcNow,

                UpdatedDate =
                    DateTime.UtcNow
            };


        _context.MassTemplates
            .Add(template);


        await _context
            .SaveChangesAsync();


        foreach (
            var item in mass.PlanItems
                .OrderBy(
                    x => x.DisplayOrder
                )
        )
        {
            _context.MassTemplateItems
                .Add(
                    new MassTemplateItem
                    {
                        MassTemplateId =
                            template.Id,

                        ItemType =
                            item.ItemType,

                        SongId =
                            item.SongId,

                        PresentationItemId =
                            item.PresentationItemId,

                        MassPart =
                            item.MassPart,

                        DisplayOrder =
                            item.DisplayOrder
                    }
                );
        }


        await _context
            .SaveChangesAsync();


        return RedirectToPage(
            "/MassTemplates/Plan",
            new
            {
                id = template.Id
            }
        );
    }


    public async Task OnGetAsync(
        string? filter,
        string? search,
        int pageNumber = 1)
    {
        /*
         * Default view is Upcoming.
         */
        ActiveFilter =
            string.IsNullOrWhiteSpace(filter)
                ? "upcoming"
                : filter
                    .Trim()
                    .ToLowerInvariant();


        /*
         * Only allow expected filters.
         */
        if (
            ActiveFilter != "upcoming" &&
            ActiveFilter != "past" &&
            ActiveFilter != "all"
        )
        {
            ActiveFilter =
                "upcoming";
        }


        Search =
            string.IsNullOrWhiteSpace(search)
                ? null
                : search.Trim();


        PageNumber =
            pageNumber < 1
                ? 1
                : pageNumber;


        var today =
            DateTime.Today;


        var query =
            _context.Masses
                .Include(x => x.Songs)
                    .ThenInclude(
                        x => x.Song
                    )
                .AsQueryable();


        /*
         * Date filter.
         */
        switch (ActiveFilter)
        {
            case "upcoming":

                query =
                    query.Where(
                        x =>
                            x.MassDate >= today
                    );

                break;


            case "past":

                query =
                    query.Where(
                        x =>
                            x.MassDate < today
                    );

                break;


            case "all":

                break;
        }


        /*
         * Search Mass Name + Venue.
         */
        if (
            !string.IsNullOrWhiteSpace(
                Search
            )
        )
        {
            var searchTerm =
                Search.ToLower();


            query =
                query.Where(
                    x =>

                        x.Name
                            .ToLower()
                            .Contains(
                                searchTerm
                            )

                        ||

                        (
                            x.Venue != null
                            &&
                            x.Venue
                                .ToLower()
                                .Contains(
                                    searchTerm
                                )
                        )
                );
        }


        /*
         * Total count before paging.
         */
        TotalCount =
            await query.CountAsync();


        TotalPages =
            TotalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    TotalCount /
                    (double)PageSize
                );


        /*
         * Protect against an invalid
         * page number after filtering.
         */
        if (
            TotalPages > 0 &&
            PageNumber > TotalPages
        )
        {
            PageNumber =
                TotalPages;
        }


        /*
         * Upcoming:
         * nearest Mass first.
         *
         * Past:
         * newest previous Mass first.
         *
         * All:
         * newest Mass first.
         */
        IOrderedQueryable<Mass>
            orderedQuery;


        if (
            ActiveFilter ==
            "upcoming"
        )
        {
            orderedQuery =
                query
                    .OrderBy(
                        x =>
                            x.MassDate
                    )
                    .ThenBy(
                        x =>
                            x.Name
                    );
        }
        else
        {
            orderedQuery =
                query
                    .OrderByDescending(
                        x =>
                            x.MassDate
                    )
                    .ThenBy(
                        x =>
                            x.Name
                    );
        }


        Masses =
            await orderedQuery

                .Skip(
                    (PageNumber - 1)
                    * PageSize
                )

                .Take(
                    PageSize
                )

                .ToListAsync();
    }


    public async Task<IActionResult>
        OnGetDuplicateAsync(
            int id)
    {
        var sourceMass =
            await _context.Masses
                .FirstOrDefaultAsync(
                    x => x.Id == id
                );


        if (sourceMass is null)
        {
            return NotFound();
        }


        var sourceSelections =
            await _context.MassSongs

                .Where(
                    x =>
                        x.MassId == id
                )

                .OrderBy(
                    x =>
                        x.DisplayOrder
                )

                .ToListAsync();


        var sourcePlanItems =
            await _context.MassPlanItems

                .Where(
                    x =>
                        x.MassId == id
                )

                .OrderBy(
                    x =>
                        x.DisplayOrder
                )

                .ToListAsync();


        var newMass =
            new Mass
            {
                Name =
                    $"{sourceMass.Name} Copy",

                MassDate =
                    sourceMass
                        .MassDate
                        .AddDays(7),

                Venue =
                    sourceMass.Venue,

                MassIntroduction =
                    sourceMass
                        .MassIntroduction,

                Notes =
                    sourceMass.Notes,

                PresentationBackgroundPath =
                    sourceMass
                        .PresentationBackgroundPath,

                CreatedDate =
                    DateTime.UtcNow,

                UpdatedDate =
                    DateTime.UtcNow
            };


        _context.Masses
            .Add(newMass);


        await _context
            .SaveChangesAsync();


        /*
         * Copy song selections.
         */
        foreach (
            var selection
            in sourceSelections
        )
        {
            _context.MassSongs
                .Add(
                    new MassSong
                    {
                        MassId =
                            newMass.Id,

                        SongId =
                            selection.SongId,

                        MassPart =
                            selection.MassPart,

                        DisplayOrder =
                            selection
                                .DisplayOrder
                    }
                );
        }


        /*
         * Copy unified presentation
         * plan order.
         */
        foreach (
            var item
            in sourcePlanItems
        )
        {
            _context.MassPlanItems
                .Add(
                    new MassPlanItem
                    {
                        MassId =
                            newMass.Id,

                        ItemType =
                            item.ItemType,

                        SongId =
                            item.SongId,

                        PresentationItemId =
                            item
                                .PresentationItemId,

                        MassPart =
                            item.MassPart,

                        DisplayOrder =
                            item
                                .DisplayOrder
                    }
                );
        }


        await _context
            .SaveChangesAsync();


        return RedirectToPage(
            "Edit",
            new
            {
                id = newMass.Id
            }
        );
    }
    private static string MakeSafeFileName(string value)
    {
        var invalidChars =
            Path.GetInvalidFileNameChars();

        var cleaned =
            new string(
                value
                    .Trim()
                    .Select(ch =>
                        invalidChars.Contains(ch)
                            ? '-'
                            : ch)
                    .ToArray());

        while (cleaned.Contains("--"))
        {
            cleaned =
                cleaned.Replace("--", "-");
        }

        return cleaned.Trim('-', ' ');
    }
}