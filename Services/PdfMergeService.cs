using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace choir_music_system.Services;

public class PdfMergeService
{
    public void Merge(
        IEnumerable<string> sourceFiles,
        string outputFile)
    {
        using var outputDocument = new PdfDocument();

        foreach (var sourceFile in sourceFiles)
        {
            using var inputDocument = PdfReader.Open(
                sourceFile,
                PdfDocumentOpenMode.Import
            );

            for (var i = 0; i < inputDocument.PageCount; i++)
            {
                outputDocument.AddPage(inputDocument.Pages[i]);
            }
        }

        outputDocument.Save(outputFile);
    }
}