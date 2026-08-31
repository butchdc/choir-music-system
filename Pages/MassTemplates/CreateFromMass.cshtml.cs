using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.MassTemplates;

public class CreateFromMassModel : PageModel
{
    private readonly ChoirDbContext _context;

    public CreateFromMassModel(ChoirDbContext context)
    {
        _context = context;
    }

    public Mass Mass { get; set; } = null!;

    [BindProperty]
    public string TemplateName { get; set; } = string.Empty;

    [BindProperty]
    public string? Notes { get; set; }

    public async Task<IActionResult> OnGetAsync(int massId)
    {
        var mass = await _context.Masses
            .FirstOrDefaultAsync(x => x.Id == massId);

        if (mass is null)
        {
            return NotFound();
        }

        Mass = mass;
        TemplateName = $"{mass.Name} Template";
        Notes = mass.Notes;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int massId)
    {
        var mass = await _context.Masses
            .Include(x => x.PlanItems)
            .FirstOrDefaultAsync(x => x.Id == massId);

        if (mass is null)
        {
            return NotFound();
        }

        Mass = mass;

        if (string.IsNullOrWhiteSpace(TemplateName))
        {
            ModelState.AddModelError(
                nameof(TemplateName),
                "Template name is required.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var template = new MassTemplate
        {
            Name = TemplateName.Trim(),
            Notes = Notes,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        _context.MassTemplates.Add(template);

        await _context.SaveChangesAsync();

        foreach (var item in mass.PlanItems
            .OrderBy(x => x.DisplayOrder))
        {
            _context.MassTemplateItems.Add(
                new MassTemplateItem
                {
                    MassTemplateId = template.Id,
                    ItemType = item.ItemType,
                    SongId = item.SongId,
                    PresentationItemId =
                        item.PresentationItemId,
                    MassPart = item.MassPart,
                    DisplayOrder = item.DisplayOrder
                });
        }

        await _context.SaveChangesAsync();

        return RedirectToPage(
            "./Plan",
            new { id = template.Id });
    }
}