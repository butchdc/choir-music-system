using choir_music_system.Data;
using choir_music_system.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.Masses;

public class MusicPackModel : PageModel
{
    private readonly ChoirDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly PdfMergeService _pdfMergeService;

    public MusicPackModel(
        ChoirDbContext context,
        IWebHostEnvironment environment,
        PdfMergeService pdfMergeService)
    {
        _context = context;
        _environment = environment;
        _pdfMergeService = pdfMergeService;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var mass = await _context.Masses
            .FirstOrDefaultAsync(x => x.Id == id);

        if (mass is null)
        {
            return NotFound();
        }

        var selections = await _context.MassSongs
            .Where(x => x.MassId == id)
            .Include(x => x.Song)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        if (selections.Count == 0)
        {
            return RedirectToPage(
                "Plan",
                new { id }
            );
        }

        // Same PDF may be assigned to several Mass parts.
        // Only include it once in the generated pack.
        var uniqueSongs = selections
            .GroupBy(x => x.SongId)
            .Select(x => x.First())
            .ToList();

        var sourceFiles = uniqueSongs
            .Select(x =>
                Path.Combine(
                    _environment.ContentRootPath,
                    x.Song.PdfPath
                ))
            .Where(System.IO.File.Exists)
            .ToList();

        if (sourceFiles.Count == 0)
        {
            return NotFound();
        }

        var generatedFolder = Path.Combine(
            _environment.ContentRootPath,
            "Storage",
            "Generated",
            "MusicPacks"
        );

        Directory.CreateDirectory(generatedFolder);

        var safeName = string.Join(
            "-",
            mass.Name.Split(
                Path.GetInvalidFileNameChars(),
                StringSplitOptions.RemoveEmptyEntries
            )
        );

        var downloadFileName =
            $"{safeName}-{mass.MassDate:yyyy-MM-dd}.pdf";

        var outputPath = Path.Combine(
            generatedFolder,
            $"{Guid.NewGuid():N}.pdf"
        );

        _pdfMergeService.Merge(
            sourceFiles,
            outputPath
        );

        var stream = new FileStream(
            outputPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read
        );

        return File(
            stream,
            "application/pdf",
            downloadFileName
        );
    }
}