using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace choir_music_system.Pages.PresentationLibrary;

[Authorize(Policy = "AdminOnly")]
public class DeleteModel : PageModel
{
    private readonly ChoirDbContext _context;

    public DeleteModel(ChoirDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public PresentationItem Item { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await _context.PresentationItems
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        Item = item;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var item = await _context.PresentationItems
            .FirstOrDefaultAsync(x => x.Id == Item.Id);

        if (item is null)
        {
            return NotFound();
        }

        // Soft delete
        item.IsActive = false;
        item.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(ReturnUrl) &&
            Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return RedirectToPage("./Index");
    }
}