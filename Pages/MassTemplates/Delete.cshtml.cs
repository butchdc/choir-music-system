using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.MassTemplates;

public class DeleteModel : PageModel
{
    private readonly ChoirDbContext _context;

    public DeleteModel(ChoirDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public MassTemplate Template { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var template = await _context.MassTemplates
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (template is null)
        {
            return NotFound();
        }

        Template = template;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var template = await _context.MassTemplates
            .FirstOrDefaultAsync(x =>
                x.Id == Template.Id &&
                x.IsActive);

        if (template is null)
        {
            return NotFound();
        }

        template.IsActive = false;
        template.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(ReturnUrl) &&
            Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return RedirectToPage("./Index");
    }
}