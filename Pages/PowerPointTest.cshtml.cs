using choir_music_system.Data;
using choir_music_system.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages;

public class PowerPointTestModel : PageModel
{
    private readonly PowerPointService _powerPointService;
    private readonly ChoirDbContext _context;

    public PowerPointTestModel(
        PowerPointService powerPointService,
        ChoirDbContext context)
    {
        _powerPointService = powerPointService;
        _context = context;
    }

    public List<string> Layouts { get; set; } = new();
    public List<string> Placeholders { get; set; } = new();

    [BindProperty]
    public int SongId { get; set; }

    public async Task OnGetAsync()
    {
        Layouts = _powerPointService.GetTemplateLayouts();
        Placeholders = _powerPointService.GetTemplatePlaceholderInfo();
    }

    public async Task<IActionResult> OnPostGenerateAsync()
    {
        var song = await _context.Songs
            .FirstOrDefaultAsync(x => x.Id == SongId);

        if (song is null)
        {
            return NotFound();
        }

        var filePath =
            _powerPointService.GenerateSongPresentation(song);

        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            $"{song.Title}.pptx");
    }
}