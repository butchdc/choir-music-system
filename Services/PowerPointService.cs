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

        var outputFolder = Path.GetTempPath();

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
    public string GenerateMassPresentation(Mass mass)
    {
        string? backgroundPath = null;

        if (!string.IsNullOrWhiteSpace(
            mass.PresentationBackgroundPath))
        {
            backgroundPath = Path.Combine(
                _environment.ContentRootPath,
                mass.PresentationBackgroundPath);

            if (!File.Exists(backgroundPath))
            {
                throw new FileNotFoundException(
                    "Presentation background image was not found.",
                    backgroundPath);
            }
        }

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

        var outputFolder = Path.GetTempPath();

        var safeName = string.Join(
            "_",
            mass.Name.Split(Path.GetInvalidFileNameChars()));

        var outputPath = Path.Combine(
            outputFolder,
            $"{safeName}-{mass.MassDate:yyyyMMdd}-{Guid.NewGuid():N}.pptx");

        File.Copy(
            templatePath,
            outputPath,
            overwrite: true);

        using var document =
            PresentationDocument.Open(outputPath, true);

        var presentationPart = document.PresentationPart
            ?? throw new InvalidOperationException(
                "Presentation part not found.");

        if (!string.IsNullOrWhiteSpace(backgroundPath))
        {
            ApplyMassBackgroundToLayouts(
                presentationPart,
                backgroundPath,
                mass);
        }

        var presentation = presentationPart.Presentation;

        var slideIdList =
            presentation.SlideIdList
            ?? presentation.AppendChild(new P.SlideIdList());

        // Remove any sample slides from the template.
        foreach (var slideId in
                 slideIdList.Elements<P.SlideId>().ToList())
        {
            var relationshipId = slideId.RelationshipId?.Value;

            if (!string.IsNullOrWhiteSpace(relationshipId))
            {
                var slidePart =
                    presentationPart.GetPartById(relationshipId)
                    as SlidePart;

                if (slidePart != null)
                {
                    presentationPart.DeletePart(slidePart);
                }
            }

            slideId.Remove();
        }

        uint nextSlideId = 256;
        string? currentMassPart = null;

        foreach (var planItem in mass.PlanItems
                     .OrderBy(x => x.DisplayOrder))
        {

            // -------------------------------------------------
            // MASS TITLE
            // -------------------------------------------------

            if (string.Equals(
                    planItem.ItemType,
                    "MassTitle",
                    StringComparison.OrdinalIgnoreCase))
            {
                var titleLayout =
                    FindLayout(
                        presentationPart,
                        "Title")
                    ?? throw new InvalidOperationException(
                        "Layout 'Title' was not found.");

                var titleSlidePart =
                    presentationPart.AddNewPart<SlidePart>();

                titleSlidePart.Slide =
                    CreateSlideFromLayout(titleLayout);

                titleSlidePart.AddPart(titleLayout);

                SetShapeText(
                    titleSlidePart.Slide,
                    "Title",
                    mass.Name);

                SetShapeText(
                    titleSlidePart.Slide,
                    "Date",
                    mass.MassDate.ToString(
                        "dddd, d MMMM yyyy"));

                SetShapeText(
                    titleSlidePart.Slide,
                    "Subtitle",
                    mass.MassIntroduction ?? string.Empty);

                titleSlidePart.Slide.Save();

                slideIdList.Append(
                    new P.SlideId
                    {
                        Id = nextSlideId++,
                        RelationshipId =
                            presentationPart.GetIdOfPart(
                                titleSlidePart)
                    });

                currentMassPart = null;

                continue;
            }
            // -------------------------------------------------
            // Divider when Mass Part changes
            // -------------------------------------------------

            var needsDivider =
                !string.IsNullOrWhiteSpace(planItem.MassPart) &&
                !string.Equals(
                    currentMassPart,
                    planItem.MassPart,
                    StringComparison.OrdinalIgnoreCase);

            if (needsDivider)
            {
                currentMassPart = planItem.MassPart;

                var dividerLayout =
                    FindLayout(presentationPart, "Divider")
                    ?? throw new InvalidOperationException(
                        "Layout 'Divider' was not found.");

                var dividerSlidePart =
                    presentationPart.AddNewPart<SlidePart>();

                dividerSlidePart.Slide =
                    CreateSlideFromLayout(dividerLayout);

                dividerSlidePart.AddPart(dividerLayout);

                dividerSlidePart.Slide.Save();

                slideIdList.Append(
                    new P.SlideId
                    {
                        Id = nextSlideId++,
                        RelationshipId =
                            presentationPart.GetIdOfPart(
                                dividerSlidePart)
                    });
            }

            // -------------------------------------------------
            // SONG
            // -------------------------------------------------

            if (planItem.ItemType == "Song" &&
                planItem.Song is not null)
            {
                var song = planItem.Song;

                var blocks =
                    ParseLyrics(song.PresentationLyrics);

                // Song without lyrics still gets a title slide.
                if (blocks.Count == 0)
                {
                    blocks.Add((
                        Titled: true,
                        Text: string.Empty
                    ));
                }

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
                        CreateSlideFromLayout(layoutPart);

                    slidePart.AddPart(layoutPart);

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

                        if (!string.IsNullOrWhiteSpace(
                            song.Composer))
                        {
                            footerParts.Add(song.Composer);
                        }

                        if (!string.IsNullOrWhiteSpace(
                            song.OneLicenseNumber))
                        {
                            footerParts.Add(
                                $"OneLicense #{song.OneLicenseNumber}");
                        }

                        if (!string.IsNullOrWhiteSpace(
                            song.Publisher))
                        {
                            footerParts.Add(song.Publisher);
                        }

                        if (!string.IsNullOrWhiteSpace(
                            song.CopyrightText))
                        {
                            footerParts.Add(
                                song.CopyrightText);
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

                    slideIdList.Append(
                        new P.SlideId
                        {
                            Id = nextSlideId++,
                            RelationshipId =
                                presentationPart.GetIdOfPart(
                                    slidePart)
                        });
                }

                continue;
            }

            // -------------------------------------------------
            // PRESENTATION LIBRARY ITEM
            // -------------------------------------------------
            if (planItem.ItemType == "Presentation" &&
                planItem.PresentationItem is not null)
            {
                var item = planItem.PresentationItem;

                var blocks = ParseLyrics(item.PresentationText);

                if (blocks.Count == 0)
                {
                    blocks.Add((
                        Titled: true,
                        Text: item.PresentationText ?? string.Empty
                    ));
                }

                foreach (var block in blocks)
                {
                    var layoutName =
                        !string.IsNullOrWhiteSpace(item.PowerPointLayout)
                            ? item.PowerPointLayout.Trim()
                            : block.Titled
                                ? "Presentation - Title + Text"
                                : "Presentation - Text";

                    var layoutPart =
                        FindLayout(
                            presentationPart,
                            layoutName)
                        ?? throw new InvalidOperationException(
                            $"Layout '{layoutName}' was not found.");

                    var slidePart =
                        presentationPart.AddNewPart<SlidePart>();

                    slidePart.Slide =
                        CreateSlideFromLayout(layoutPart);

                    slidePart.AddPart(layoutPart);

                    if (block.Titled)
                    {
                        SetShapeText(
                            slidePart.Slide,
                            "Title",
                            item.Title);

                        SetShapeText(
                            slidePart.Slide,
                            "Text",
                            block.Text);
                    }
                    else
                    {
                        SetShapeText(
                            slidePart.Slide,
                            "Text",
                            block.Text);
                    }

                    slidePart.Slide.Save();

                    slideIdList.Append(
                        new P.SlideId
                        {
                            Id = nextSlideId++,
                            RelationshipId =
                                presentationPart.GetIdOfPart(
                                    slidePart)
                        });
                }
            }

        }
        presentation.Save();

        return outputPath;
    }

    private static P.Slide CreateSlideFromLayout(
    SlideLayoutPart layoutPart)
    {
        var slide =
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

        foreach (var shape in
                 layoutPart.SlideLayout.CommonSlideData
                     .ShapeTree.Elements<P.Shape>())
        {
            slide.CommonSlideData.ShapeTree
                .Append(shape.CloneNode(true));
        }

        return slide;
    }

    private static void AddBackgroundToLayout(
    SlideLayoutPart layoutPart,
    string imagePath,
    int transparencyPercent)
    {
        if (!File.Exists(imagePath))
            return;

        var extension = Path.GetExtension(imagePath).ToLowerInvariant();

        var imageType = extension switch
        {
            ".png" => ImagePartType.Png,
            ".jpg" => ImagePartType.Jpeg,
            ".jpeg" => ImagePartType.Jpeg,
            _ => throw new InvalidOperationException(
                "Unsupported background image type.")
        };

        var imagePart = layoutPart.AddImagePart(imageType);

        using (var stream = File.OpenRead(imagePath))
        {
            imagePart.FeedData(stream);
        }

        var relationshipId =
            layoutPart.GetIdOfPart(imagePart);

        var blip = new A.Blip
        {
            Embed = relationshipId
        };

        // 0 transparency = 100% opacity
        // 15 transparency = 85% opacity
        var opacity =
            100000 - (transparencyPercent * 1000);

        blip.Append(
            new A.AlphaModulationFixed
            {
                Amount = opacity
            });

        var picture = new P.Picture(
            new P.NonVisualPictureProperties(
                new P.NonVisualDrawingProperties
                {
                    Id = 5000U,
                    Name = "Presentation Background"
                },
                new P.NonVisualPictureDrawingProperties(
                    new A.PictureLocks
                    {
                        NoChangeAspect = true
                    }),
                new P.ApplicationNonVisualDrawingProperties()
            ),

            new P.BlipFill(
                blip,
                new A.Stretch(
                    new A.FillRectangle())
            ),

            new P.ShapeProperties(
                new A.Transform2D(
                    new A.Offset
                    {
                        X = 0,
                        Y = 0
                    },
                    new A.Extents
                    {
                        Cx = 12192000,
                        Cy = 6858000
                    }
                ),
                new A.PresetGeometry(
                    new A.AdjustValueList())
                {
                    Preset = A.ShapeTypeValues.Rectangle
                }
            )
        );

        var shapeTree =
            layoutPart.SlideLayout.CommonSlideData.ShapeTree;

        var groupProperties =
            shapeTree.GetFirstChild<P.GroupShapeProperties>();

        if (groupProperties != null)
        {
            shapeTree.InsertAfter(
                picture,
                groupProperties);
        }
        else
        {
            shapeTree.PrependChild(picture);
        }

        layoutPart.SlideLayout.Save();
    }
    private static void ApplyMassBackgroundToLayouts(
        PresentationPart presentationPart,
        string backgroundPath,
        Mass mass)
    {
        var titleLayout =
            FindLayout(
                presentationPart,
                "Title");

        var dividerLayout =
            FindLayout(
                presentationPart,
                "Divider");

        var titledSongLayout =
            FindLayout(
                presentationPart,
                "Song - Title + Lyrics");

        var lyricsLayout =
            FindLayout(
                presentationPart,
                "Song - Lyrics");

        var presentationTitleLayout =
            FindLayout(
                presentationPart,
                "Presentation - Title + Text");

        var presentationTextLayout =
            FindLayout(
                presentationPart,
                "Presentation - Text");

        if (titleLayout != null)
        {
            AddBackgroundToLayout(
                titleLayout,
                backgroundPath,
                0);
        }

        if (dividerLayout != null)
        {
            AddBackgroundToLayout(
                dividerLayout,
                backgroundPath,
                0);
        }

        if (titledSongLayout != null)
        {
            AddBackgroundToLayout(
                titledSongLayout,
                backgroundPath,
                85);
        }

        if (lyricsLayout != null)
        {
            AddBackgroundToLayout(
                lyricsLayout,
                backgroundPath,
                85);
        }

        if (presentationTitleLayout != null)
        {
            AddBackgroundToLayout(
                presentationTitleLayout,
                backgroundPath,
                85);
        }

        if (presentationTextLayout != null)
        {
            AddBackgroundToLayout(
                presentationTextLayout,
                backgroundPath,
                85);
        }

        var customLayoutNames =
            mass.PlanItems
                .Where(x =>
                    x.ItemType == "Presentation" &&
                    x.PresentationItem != null &&
                    !string.IsNullOrWhiteSpace(
                        x.PresentationItem.PowerPointLayout))
                .Select(x =>
                    x.PresentationItem!
                        .PowerPointLayout!
                        .Trim())
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        foreach (var customLayoutName in customLayoutNames)
        {
            var customLayout =
                FindLayout(
                    presentationPart,
                    customLayoutName);

            if (customLayout == null)
            {
                throw new InvalidOperationException(
                    $"Layout '{customLayoutName}' was not found.");
            }

            AddBackgroundToLayout(
                customLayout,
                backgroundPath,
                85);
        }
    }
}