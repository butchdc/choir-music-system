using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace choir_music_system.Pages.Masses;

public class CreateModel : PageModel
{
    private readonly ChoirDbContext _context;

    public CreateModel(ChoirDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Mass Mass { get; set; } = new()
    {
        MassDate = DateTime.Today
    };

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Mass.CreatedDate = DateTime.UtcNow;
        Mass.UpdatedDate = DateTime.UtcNow;

        _context.Masses.Add(Mass);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}