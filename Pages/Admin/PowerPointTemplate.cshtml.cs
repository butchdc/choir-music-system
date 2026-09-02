using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace choir_music_system.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class PowerPointTemplateModel : PageModel
{
    private readonly IWebHostEnvironment _environment;

    private static readonly string[] RequiredLayouts =
    {
        "Title",
        "Divider",
        "Song - Title + Lyrics",
        "Song - Lyrics",
        "Presentation - Title + Text",
        "Presentation - Text"
    };

    public PowerPointTemplateModel(
        IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [BindProperty]
    public IFormFile? TemplateFile { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public bool TemplateExists { get; set; }

    public DateTime? LastModified { get; set; }

    public List<string> CurrentLayouts { get; set; } = new();

    private string TemplateFolder =>
        Path.Combine(
            _environment.ContentRootPath,
            "Storage",
            "PowerPointTemplates");

    private string TemplatePath =>
        Path.Combine(
            TemplateFolder,
            "Template.pptx");

    public void OnGet()
    {
        LoadTemplateInfo();
    }

    public IActionResult OnGetDownload()
    {
        if (!System.IO.File.Exists(TemplatePath))
        {
            return NotFound();
        }

        var bytes =
            System.IO.File.ReadAllBytes(TemplatePath);

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "Template.pptx");
    }

    public async Task<IActionResult> OnPostUploadAsync()
    {
        if (TemplateFile == null ||
            TemplateFile.Length == 0)
        {
            StatusMessage =
                "Please select a PowerPoint template.";

            return RedirectToPage();
        }

        if (!string.Equals(
            Path.GetExtension(TemplateFile.FileName),
            ".pptx",
            StringComparison.OrdinalIgnoreCase))
        {
            StatusMessage =
                "Only .pptx PowerPoint templates are supported.";

            return RedirectToPage();
        }

        Directory.CreateDirectory(TemplateFolder);

        var tempPath =
            Path.Combine(
                Path.GetTempPath(),
                $"choir-template-{Guid.NewGuid():N}.pptx");

        try
        {
            await using (var stream =
                new FileStream(
                    tempPath,
                    FileMode.Create,
                    FileAccess.Write))
            {
                await TemplateFile.CopyToAsync(stream);
            }

            var validation =
                ValidateTemplate(tempPath);

            if (!validation.IsValid)
            {
                StatusMessage =
                    validation.Message;

                return RedirectToPage();
            }

            // Archive existing template first.
            if (System.IO.File.Exists(TemplatePath))
            {
                var archiveFolder =
                    Path.Combine(
                        TemplateFolder,
                        "Archive");

                Directory.CreateDirectory(
                    archiveFolder);

                var timestamp =
                    DateTime.Now.ToString(
                        "yyyyMMdd-HHmmss");

                var archivePath =
                    Path.Combine(
                        archiveFolder,
                        $"Template-{timestamp}.pptx");

                System.IO.File.Copy(
                    TemplatePath,
                    archivePath,
                    overwrite: false);
            }

            // Replace live template.
            System.IO.File.Copy(
                tempPath,
                TemplatePath,
                overwrite: true);

            StatusMessage =
                "PowerPoint template updated successfully.";

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Template update failed: {ex.Message}";

            return RedirectToPage();
        }
        finally
        {
            if (System.IO.File.Exists(tempPath))
            {
                System.IO.File.Delete(tempPath);
            }
        }
    }

    private void LoadTemplateInfo()
    {
        TemplateExists =
            System.IO.File.Exists(TemplatePath);

        if (!TemplateExists)
        {
            return;
        }

        LastModified =
            System.IO.File.GetLastWriteTime(
                TemplatePath);

        try
        {
            using var document =
                PresentationDocument.Open(
                    TemplatePath,
                    false);

            var presentationPart =
                document.PresentationPart;

            if (presentationPart == null)
            {
                return;
            }

            CurrentLayouts =
                presentationPart
                    .SlideMasterParts
                    .SelectMany(x =>
                        x.SlideLayoutParts)
                    .Select(x =>
                        x.SlideLayout?
                            .CommonSlideData?
                            .Name?
                            .Value)
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!)
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList();
        }
        catch
        {
            CurrentLayouts = new();
        }
    }

    private static (
        bool IsValid,
        string Message)
        ValidateTemplate(
            string templatePath)
    {
        try
        {
            using var document =
                PresentationDocument.Open(
                    templatePath,
                    false);

            var presentationPart =
                document.PresentationPart;

            if (presentationPart == null)
            {
                return (
                    false,
                    "The uploaded file does not contain a valid PowerPoint presentation.");
            }

            var layouts =
                presentationPart
                    .SlideMasterParts
                    .SelectMany(x =>
                        x.SlideLayoutParts)
                    .Select(x =>
                        x.SlideLayout?
                            .CommonSlideData?
                            .Name?
                            .Value)
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!)
                    .ToHashSet(
                        StringComparer.OrdinalIgnoreCase);

            var missing =
                RequiredLayouts
                    .Where(x =>
                        !layouts.Contains(x))
                    .ToList();

            if (missing.Count > 0)
            {
                return (
                    false,
                    "Template was not replaced. Missing required layouts: " +
                    string.Join(", ", missing));
            }

            return (
                true,
                "Template is valid.");
        }
        catch
        {
            return (
                false,
                "The uploaded file is not a valid PowerPoint .pptx template.");
        }
    }
}