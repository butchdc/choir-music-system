using choir_music_system.Data;
using choir_music_system.Models;
using choir_music_system.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace choir_music_system.Pages.PresentationLibrary;

public class CreateModel : PageModel
{
    private readonly ChoirDbContext _context;
    private readonly PowerPointService _powerPointService;

    public CreateModel(
        ChoirDbContext context,
        PowerPointService powerPointService)
    {
        _context = context;
        _powerPointService = powerPointService;
    }

    [BindProperty]
    public PresentationItem Item { get; set; } = new();

    public List<string> PowerPointLayouts { get; set; } = new();

    public void OnGet()
    {
        LoadPowerPointLayouts();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            LoadPowerPointLayouts();
            return Page();
        }

        Item.PowerPointLayout =
            string.IsNullOrWhiteSpace(Item.PowerPointLayout)
                ? null
                : Item.PowerPointLayout.Trim();

        Item.CreatedDate = DateTime.UtcNow;
        Item.UpdatedDate = DateTime.UtcNow;
        Item.IsActive = true;

        _context.PresentationItems.Add(Item);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }

    private void LoadPowerPointLayouts()
    {
        PowerPointLayouts = _powerPointService
            .GetTemplateLayouts()
            .OrderBy(x => x)
            .ToList();
    }
}