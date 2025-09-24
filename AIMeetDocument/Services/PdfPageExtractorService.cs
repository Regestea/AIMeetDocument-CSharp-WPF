using System.IO;
using AIMeetDocument.DTOs;
using PdfiumViewer;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System;


namespace AIMeetDocument.Services;

public class PdfPageExtractorService : IDisposable
{
    /// <summary>
    /// Extracts specific page ranges from multiple PDFs and saves them as PNG files.
    /// </summary>
    /// <param name="pdfFilePath">ull path to the source PDF file.</param>
    /// <param name="pageRanges">A list of PageRange objects specifying which pages to extract.</param>
    /// <returns>A list of strings containing the full paths to the saved PNG files.</returns>
    public List<string> ExtractPages(string pdfFilePath, List<PageRange> pageRanges)
    {
        string outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImageCache");
        Directory.CreateDirectory(outputDirectory);
        var savedFilePaths = new List<string>();
        const int dpi = 500; // Image quality

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
                            Console.WriteLine(
                                $"Warning: Page number {pageNumber} is out of range in file {pdfFilePath}. Skipping.");
                            continue;
                        }

                        // Sanitize file name to remove invalid characters
                        string baseFileName = Path.GetFileNameWithoutExtension(pdfFilePath);
                        string safeBaseFileName = Regex.Replace(baseFileName, "[\\/:*?\"<>|]", "_");
                        string fileName = $"{Guid.NewGuid()}.png";
                        string outputPath = Path.Combine(outputDirectory, fileName);
                        int pageIndex = pageNumber - 1;

                        // Calculate pixel dimensions for DPI
                        var size = document.PageSizes[pageIndex]; // SizeF in points (1/72 inch)
                        int widthPx = (int)(size.Width / 72.0 * dpi);
                        int heightPx = (int)(size.Height / 72.0 * dpi);
                        // Skip rendering if image is too small
                        if (widthPx < 3 || heightPx < 3)
                        {
                            Console.WriteLine($"Warning: Image too small to scale!! ({widthPx}x{heightPx} vs min width/height of 3) for page {pageNumber} in file {pdfFilePath}. Skipping.");
                            continue;
                        }

                        // Fix for GDI+ error: delete file if it exists before saving
                        try
                        {
                            if (File.Exists(outputPath))
                            {
                                File.Delete(outputPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to delete existing file {outputPath}: {ex.Message}");
                            continue;
                        }

                        try
                        {
                            using (var image = document.Render(pageIndex, widthPx, heightPx, dpi, dpi, true))
                            using (var ms = new MemoryStream())
                            {
                                image.Save(ms, ImageFormat.Png);
                                ms.Position = 0;
                                using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
                                {
                                    ms.CopyTo(fs);
                                }
                            }
                            savedFilePaths.Add(outputPath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to save image to {outputPath}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred processing {pdfFilePath}: {ex.Message}");
        }


        return savedFilePaths;
    }

    /// <summary>
    /// Deletes all files in the ImageCache directory.
    /// </summary>
    public void ClearImageCacheDirectory()
    {
        string outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImageCache");
        if (!Directory.Exists(outputDirectory))
            return;
        try
        {
            var files = Directory.GetFiles(outputDirectory);
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete file {file}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to clear ImageCache directory: {ex.Message}");
        }
    }

    public void Dispose()
    {
        ClearImageCacheDirectory();
    }
}