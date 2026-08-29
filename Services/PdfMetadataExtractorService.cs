using choir_music_system.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace choir_music_system.Services;

public class PdfMetadataExtractorService
{
    public MusicSheetMetadata Extract(string pdfPath)
    {
        var result = new MusicSheetMetadata();

        using var document = PdfDocument.Open(pdfPath);

        if (document.NumberOfPages == 0)
        {
            return result;
        }

        var page = document.GetPage(1);
        var words = page.GetWords().ToList();

        if (words.Count == 0)
        {
            return result;
        }

        var pageHeight = (double)page.Height;

        // Only inspect the top 10% of page 1.
        var headerWords = words
            .Where(w =>
                w.BoundingBox.Bottom >
                pageHeight * 0.90)
            .Where(w => IsReadableText(w.Text))
            .ToList();

        if (headerWords.Count == 0)
        {
            return result;
        }

        // Group words that visually belong on the same line.
        var groups = GroupWordsByLine(headerWords);

        if (groups.Count == 0)
        {
            return result;
        }

        // The title is normally the most visually prominent line.
        var titleGroup = groups
            .OrderByDescending(g =>
                g.Average(w => w.BoundingBox.Height))
            .First();

        result.Title = JoinWords(titleGroup);

        return result;
    }

    private static List<List<Word>> GroupWordsByLine(
        List<Word> words)
    {
        const double yTolerance = 4;

        var groups = new List<List<Word>>();

        foreach (var word in words
                     .OrderByDescending(w =>
                         w.BoundingBox.Bottom)
                     .ThenBy(w =>
                         w.BoundingBox.Left))
        {
            var group = groups.FirstOrDefault(g =>
                Math.Abs(
                    g.Average(x =>
                        x.BoundingBox.Bottom) -
                    word.BoundingBox.Bottom
                ) <= yTolerance
            );

            if (group is null)
            {
                groups.Add(
                    new List<Word>
                    {
                        word
                    }
                );
            }
            else
            {
                group.Add(word);
            }
        }

        return groups;
    }

    private static string JoinWords(
        IEnumerable<Word> words)
    {
        return string.Join(
            " ",
            words
                .OrderBy(w => w.BoundingBox.Left)
                .Select(w => w.Text)
        ).Trim();
    }

    private static bool IsReadableText(
        string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var value = text.Trim();

        // Keep separators because "/" is used for
        // Composer / Arranger.
        if (value == "/" || value == "&")
        {
            return true;
        }

        return value.Any(char.IsLetter);
    }

    private static bool LooksLikeMusicLine(
        List<Word> words)
    {
        var readableWords = words
            .Select(w => w.Text.Trim())
            .Where(x =>
                !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (readableWords.Count == 0)
        {
            return true;
        }

        var chordCount = readableWords
            .Count(LooksLikeChord);

        // If most of the line looks like chords,
        // don't use it as a composer credit.
        return chordCount >
               readableWords.Count / 2;
    }

    private static bool LooksLikeChord(
        string text)
    {
        var value = text.Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (value.Length > 10)
        {
            return false;
        }

        const string chordLetters =
            "ABCDEFG";

        if (!chordLetters.Contains(
                value[0],
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return value.Length <= 6;
    }

    private static void ApplyComposerAndArrangement(
        MusicSheetMetadata metadata,
        string credit)
    {
        if (string.IsNullOrWhiteSpace(credit))
        {
            return;
        }

        credit = credit.Trim();

        var slashIndex = credit.IndexOf('/');

        if (slashIndex >= 0)
        {
            var composer =
                credit[..slashIndex].Trim();

            var arranger =
                credit[(slashIndex + 1)..].Trim();

            metadata.Composer =
                string.IsNullOrWhiteSpace(composer)
                    ? null
                    : composer;

            metadata.Arrangement =
                string.IsNullOrWhiteSpace(arranger)
                    ? null
                    : arranger;
        }
        else
        {
            metadata.Composer = credit;
            metadata.Arrangement = null;
        }
    }
}