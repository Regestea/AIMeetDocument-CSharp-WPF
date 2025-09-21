using System.IO;
using AIMeetDocument.DTOs;
using PdfiumViewer;

namespace AIMeetDocument.Services;

public class PdfPageExtractorService
{
    /// <summary>
    /// Extracts specific page ranges from multiple PDFs and saves them as PNG files.
    /// </summary>
    /// <param name="pdfFilePaths">A list of full paths to the source PDF files.</param>
    /// <param name="pageRanges">A list of PageRange objects specifying which pages to extract.</param>
    /// <returns>A list of strings containing the full paths to the saved PNG files.</returns>
    public List<string> ExtractPages(List<string> pdfFilePaths, List<PageRange> pageRanges)
    {
        string outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImageCache");
        Directory.CreateDirectory(outputDirectory);
        var savedFilePaths = new List<string>();

        foreach (var pdfFilePath in pdfFilePaths)
        {
            if (!File.Exists(pdfFilePath))
            {
                Console.WriteLine($"Warning: The specified PDF file was not found: {pdfFilePath}");
                continue;
            }
            try
            {
                using (var document = PdfDocument.Load(pdfFilePath))
                {
                    int totalPages = document.PageCount;
                    foreach (var range in pageRanges)
                    {
                        for (int pageNumber = range.From; pageNumber <= range.To; pageNumber++)
                        {
                            if (pageNumber < 1 || pageNumber > totalPages)
                            {
                                Console.WriteLine($"Warning: Page number {pageNumber} is out of range in file {pdfFilePath}. Skipping.");
                                continue;
                            }
                            string fileName = $"{Path.GetFileNameWithoutExtension(pdfFilePath)}_page_{pageNumber}.png";
                            string outputPath = Path.Combine(outputDirectory, fileName);
                            int pageIndex = pageNumber - 1;
                            using (var image = document.Render(pageIndex, 300, 300, true))
                            {
                                image.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);
                            }
                            savedFilePaths.Add(outputPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred processing {pdfFilePath}: {ex.Message}");
            }
        }

        return savedFilePaths;
    }
}