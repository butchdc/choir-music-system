using choir_music_system.Data;
using choir_music_system.Models;
using choir_music_system.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.PresentationLibrary;

public class EditModel : PageModel
{
    private readonly ChoirDbContext _context;
    private readonly PowerPointService _powerPointService;

    public EditModel(
        ChoirDbContext context,
        PowerPointService powerPointService)
    {
        _context = context;
        _powerPointService = powerPointService;
    }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty]
    public PresentationItem Item { get; set; } = null!;

    public List<string> PowerPointLayouts { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await _context.PresentationItems
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item is null)
            return NotFound();

        Item = item;

        LoadPowerPointLayouts();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var existing = await _context.PresentationItems
            .FirstOrDefaultAsync(x => x.Id == Item.Id);

        if (existing is null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            LoadPowerPointLayouts();
            return Page();
        }

        existing.Title = Item.Title;
        existing.Type = Item.Type;
        existing.Language = Item.Language;
        existing.PresentationText = Item.PresentationText;

        existing.PowerPointLayout =
            string.IsNullOrWhiteSpace(Item.PowerPointLayout)
                ? null
                : Item.PowerPointLayout.Trim();

        existing.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(ReturnUrl) &&
            Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return RedirectToPage("./Index");
    }

    private void LoadPowerPointLayouts()
    {
        PowerPointLayouts = _powerPointService
            .GetTemplateLayouts()
            .OrderBy(x => x)
            .ToList();
    }
}