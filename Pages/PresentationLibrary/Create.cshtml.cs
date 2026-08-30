using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace choir_music_system.Pages.PresentationLibrary;

public class CreateModel : PageModel
{
    private readonly ChoirDbContext _context;

    public CreateModel(ChoirDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public PresentationItem Item { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Item.CreatedDate = DateTime.UtcNow;
        Item.UpdatedDate = DateTime.UtcNow;
        Item.IsActive = true;

        _context.PresentationItems.Add(Item);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}