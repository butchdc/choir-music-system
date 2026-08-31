using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.PresentationLibrary;

public class IndexModel : PageModel
{
    private readonly ChoirDbContext _context;

    public IndexModel(ChoirDbContext context)
    {
        _context = context;
    }

    public IList<PresentationItem> Items { get; set; }
        = new List<PresentationItem>();

    public string Sort { get; set; } = "type";
    public string Direction { get; set; } = "asc";

    public async Task OnGetAsync(
        string? sort,
        string? direction)
    {
        Sort = sort?.ToLowerInvariant() switch
        {
            "title" => "title",
            "type" => "type",
            "language" => "language",
            _ => "type"
        };

        Direction =
            string.Equals(
                direction,
                "desc",
                StringComparison.OrdinalIgnoreCase)
                ? "desc"
                : "asc";

        var query = _context.PresentationItems
            .Where(x => x.IsActive);

        query = (Sort, Direction) switch
        {
            ("title", "desc") =>
                query.OrderByDescending(x => x.Title),

            ("title", _) =>
                query.OrderBy(x => x.Title),

            ("language", "desc") =>
                query.OrderByDescending(x => x.Language)
                     .ThenBy(x => x.Title),

            ("language", _) =>
                query.OrderBy(x => x.Language)
                     .ThenBy(x => x.Title),

            ("type", "desc") =>
                query.OrderByDescending(x => x.Type)
                     .ThenBy(x => x.Title),

            _ =>
                query.OrderBy(x => x.Type)
                     .ThenBy(x => x.Title)
        };

        Items = await query.ToListAsync();
    }
}