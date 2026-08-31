using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.PresentationLibrary;

public class EditModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }
    private readonly ChoirDbContext _context;

    public EditModel(ChoirDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public PresentationItem Item { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await _context.PresentationItems
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item is null)
            return NotFound();

        Item = item;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var existing = await _context.PresentationItems
            .FirstOrDefaultAsync(x => x.Id == Item.Id);

        if (existing is null)
            return NotFound();

        if (!ModelState.IsValid)
            return Page();

        existing.Title = Item.Title;
        existing.Type = Item.Type;
        existing.Language = Item.Language;
        existing.PresentationText = Item.PresentationText;
        existing.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(ReturnUrl) &&
            Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return RedirectToPage("./Index");
    }
}