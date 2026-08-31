using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace choir_music_system.Pages.MassTemplates;

public class CreateModel : PageModel
{
    private readonly ChoirDbContext _context;

    public CreateModel(ChoirDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public MassTemplate Template { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Template.CreatedDate = DateTime.UtcNow;
        Template.UpdatedDate = DateTime.UtcNow;
        Template.IsActive = true;

        _context.MassTemplates.Add(Template);

        await _context.SaveChangesAsync();

        return RedirectToPage(
            "./Plan",
            new { id = Template.Id });
    }
}