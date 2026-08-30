using choir_music_system.Models;
using DocumentFormat.OpenXml.Packaging;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

namespace choir_music_system.Services;

public class PowerPointService
{
    private readonly IWebHostEnvironment _environment;

    public PowerPointService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public List<string> GetTemplateLayouts()
    {
        var templatePath = Path.Combine(
            _environment.ContentRootPath,
            "Storage",
            "PowerPointTemplates",
            "Template.pptx");

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException(
                "PowerPoint template was not found.",
                templatePath);
        }

        using var document =
            PresentationDocument.Open(templatePath, false);

        var presentationPart = document.PresentationPart
            ?? throw new InvalidOperationException(
                "The PowerPoint template does not contain a presentation.");

        var layouts = new List<string>();

        foreach (var masterPart in presentationPart.SlideMasterParts)
        {
            foreach (var layoutPart in masterPart.SlideLayoutParts)
            {
                var layout = layoutPart.SlideLayout;

                var name = layout?.CommonSlideData?.Name?.Value;

                if (!string.IsNullOrWhiteSpace(name))
                {
                    layouts.Add(name);
                }
            }
        }

        return layouts
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();
    }

    public List<string> GetTemplatePlaceholderInfo()
    {
        var templatePath = Path.Combine(
            _environment.ContentRootPath,
            "Storage",
            "PowerPointTemplates",
            "Template.pptx");

        using var document =
            PresentationDocument.Open(templatePath, false);

        var presentationPart = document.PresentationPart
            ?? throw new InvalidOperationException(
                "The PowerPoint template does not contain a presentation.");

        var results = new List<string>();

        foreach (var masterPart in presentationPart.SlideMasterParts)
        {
            foreach (var layoutPart in masterPart.SlideLayoutParts)
            {
                var layout = layoutPart.SlideLayout;

                var layoutName =
                    layout?.CommonSlideData?.Name?.Value ?? "(unnamed)";

                results.Add($"LAYOUT: {layoutName}");

                if (layout?.CommonSlideData?.ShapeTree == null)
                    continue;

                foreach (var shape in
                         layout.CommonSlideData.ShapeTree.Elements<P.Shape>())
                {
                    var placeholder =
                        shape.NonVisualShapeProperties?
                            .ApplicationNonVisualDrawingProperties?
                            .GetFirstChild<P.PlaceholderShape>();

                    var shapeName =
                        shape.NonVisualShapeProperties?
                            .NonVisualDrawingProperties?
                            .Name?.Value ?? "(unnamed)";

                    if (placeholder != null)
                    {
                        results.Add(
                            $"  Placeholder: {shapeName} | " +
                            $"Type: {placeholder.Type?.Value.ToString() ?? "Body"} | " +
                            $"Index: {placeholder.Index?.Value.ToString() ?? "-"}");
                    }
                }
            }
        }

        return results;
    }

    private static List<(bool Titled, string Text)> ParseLyrics(
        string? lyrics)
    {
        var result = new List<(bool Titled, string Text)>();

        if (string.IsNullOrWhiteSpace(lyrics))
            return result;

        var lines = lyrics
            .Replace("\r\n", "\n")
            .Split('\n');

        bool? titled = null;
        var current = new List<string>();

        void Flush()
        {
            if (titled is null || current.Count == 0)
                return;

            var text = string.Join("\n", current).Trim();

            if (!string.IsNullOrWhiteSpace(text))
            {
                result.Add((titled.Value, text));
            }

            current.Clear();
        }

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            if (trimmed.Equals(
                "[SLIDE:TITLED]",
                StringComparison.OrdinalIgnoreCase))
            {
                Flush();
                titled = true;
                continue;
            }

            if (trimmed.Equals(
                "[SLIDE]",
                StringComparison.OrdinalIgnoreCase))
            {
                Flush();
                titled = false;
                continue;
            }

            current.Add(line);
        }

        Flush();

        return result;
    }

    private static SlideLayoutPart? FindLayout(
        PresentationPart presentationPart,
        string layoutName)
    {
        foreach (var masterPart in presentationPart.SlideMasterParts)
        {
            foreach (var layoutPart in masterPart.SlideLayoutParts)
            {
                var name = layoutPart.SlideLayout?
                    .CommonSlideData?
                    .Name?
                    .Value;

                if (string.Equals(
                    name,
                    layoutName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return layoutPart;
                }
            }
        }

        return null;
    }
    private static string SanitizeForXml(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return new string(
            text.Where(ch =>
                ch == '\t' ||
                ch == '\n' ||
                ch == '\r' ||
                ch >= 0x20)
            .ToArray());
    }
    private static void SetShapeText(
        P.Slide slide,
        string shapeName,
        string text)
    {
        var shape = slide
            .CommonSlideData
            .ShapeTree
            .Elements<P.Shape>()
            .FirstOrDefault(x =>
                string.Equals(
                    x.NonVisualShapeProperties?
                        .NonVisualDrawingProperties?
                        .Name?
                        .Value,
                    shapeName,
                    StringComparison.OrdinalIgnoreCase));

        if (shape?.TextBody == null)
            return;

        shape.TextBody.RemoveAllChildren<A.Paragraph>();

        var cleanText = SanitizeForXml(text);

        var lines = cleanText
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n');

        foreach (var line in lines)
        {
            var paragraph = new A.Paragraph();

            var run = new A.Run(
                new A.RunProperties(),
                new A.Text(line));

            paragraph.Append(run);
            shape.TextBody.Append(paragraph);
        }
    }

    public string GenerateSongPresentation(Song song)
    {
        var templatePath = Path.Combine(
            _environment.ContentRootPath,
            "Storage",
            "PowerPointTemplates",
            "Template.pptx");

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException(
                "PowerPoint template was not found.",
                templatePath);
        }

        var blocks = ParseLyrics(song.PresentationLyrics);

        if (blocks.Count == 0)
        {
            blocks.Add((
                Titled: true,
                Text: string.Empty
            ));
        }

        var outputFolder = Path.Combine(
            _environment.ContentRootPath,
            "Storage",
            "Generated",
            "Presentations");

        Directory.CreateDirectory(outputFolder);

        var safeTitle = string.Join(
            "_",
            song.Title.Split(Path.GetInvalidFileNameChars()));

        var outputPath = Path.Combine(
            outputFolder,
            $"{safeTitle}-{DateTime.Now:yyyyMMddHHmmss}.pptx");

        File.Copy(
            templatePath,
            outputPath,
            overwrite: true);

        using var document =
            PresentationDocument.Open(outputPath, true);

        var presentationPart = document.PresentationPart
            ?? throw new InvalidOperationException(
                "Presentation part not found.");

        var presentation =
            presentationPart.Presentation;

        var slideIdList =
            presentation.SlideIdList
            ?? presentation.AppendChild(new P.SlideIdList());

        // Remove existing sample slides from the copied template.
        foreach (var slideId in
                 slideIdList.Elements<P.SlideId>().ToList())
        {
            var relationshipId =
                slideId.RelationshipId?.Value;

            if (!string.IsNullOrWhiteSpace(relationshipId))
            {
                var slidePart =
                    presentationPart.GetPartById(
                        relationshipId) as SlidePart;

                if (slidePart != null)
                {
                    presentationPart.DeletePart(slidePart);
                }
            }

            slideId.Remove();
        }

        uint nextSlideId = 256;

        foreach (var block in blocks)
        {
            var layoutName = block.Titled
                ? "Song - Title + Lyrics"
                : "Song - Lyrics";

            var layoutPart =
                FindLayout(
                    presentationPart,
                    layoutName)
                ?? throw new InvalidOperationException(
                    $"Layout '{layoutName}' was not found.");

            var slidePart =
                presentationPart.AddNewPart<SlidePart>();

            slidePart.Slide =
                new P.Slide(
                    new P.CommonSlideData(
                        new P.ShapeTree(
                            new P.NonVisualGroupShapeProperties(
                                new P.NonVisualDrawingProperties
                                {
                                    Id = 1U,
                                    Name = string.Empty
                                },
                                new P.NonVisualGroupShapeDrawingProperties(),
                                new P.ApplicationNonVisualDrawingProperties()
                            ),
                            new P.GroupShapeProperties()
                        )
                    ),
                    new P.ColorMapOverride(
                        new A.MasterColorMapping()
                    )
                );

            slidePart.AddPart(layoutPart);

            var sourceShapes =
                layoutPart
                    .SlideLayout
                    .CommonSlideData
                    .ShapeTree
                    .Elements<P.Shape>();

            foreach (var shape in sourceShapes)
            {
                slidePart
                    .Slide
                    .CommonSlideData
                    .ShapeTree
                    .Append(
                        shape.CloneNode(true));
            }

            if (block.Titled)
            {
                SetShapeText(
                    slidePart.Slide,
                    "Song Title",
                    song.Title);

                SetShapeText(
                    slidePart.Slide,
                    "Lyrics",
                    block.Text);

                var footerParts =
                    new List<string>();

                if (!string.IsNullOrWhiteSpace(song.Composer))
                {
                    footerParts.Add(song.Composer);
                }

                if (!string.IsNullOrWhiteSpace(
                    song.OneLicenseNumber))
                {
                    footerParts.Add(
                        $"OneLicense #{song.OneLicenseNumber}");
                }

                if (!string.IsNullOrWhiteSpace(song.Publisher))
                {
                    footerParts.Add(song.Publisher);
                }

                if (!string.IsNullOrWhiteSpace(song.CopyrightText))
                {
                    footerParts.Add(song.CopyrightText);
                }

                SetShapeText(
                    slidePart.Slide,
                    "Footer",
                    string.Join(
                        " • ",
                        footerParts));
            }
            else
            {
                SetShapeText(
                    slidePart.Slide,
                    "Lyrics",
                    block.Text);
            }

            slidePart.Slide.Save();

            var newSlideId =
                new P.SlideId
                {
                    Id = nextSlideId++,
                    RelationshipId =
                        presentationPart.GetIdOfPart(
                            slidePart)
                };

            slideIdList.Append(
                newSlideId);
        }

        presentation.Save();

        return outputPath;
    }
}