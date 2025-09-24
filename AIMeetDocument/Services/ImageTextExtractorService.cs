using AIMeetDocument.Enums;
using Tesseract;

namespace AIMeetDocument.Services;

public class ImageTextExtractorService
{
    public List<string> ReadTextFromImage(List<string> imagePaths, ImageToTextLanguage language)
    {
        var results = new List<string>();
        try
        {
            using (var engine = new TesseractEngine(@"./TesseractData", language.ToString(), EngineMode.Default))
            {
                foreach (var imagePath in imagePaths)
                {
                    try
                    {
                        using (var img = Pix.LoadFromFile(imagePath))
                        using (var page = engine.Process(img))
                        {
                            results.Add(page.GetText());
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"OCR Error for {imagePath}: {ex.Message}");
                        results.Add(string.Empty); // Use empty string for failed OCR
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected OCR Error: {e.Message}");
            // If the engine fails, return a list of nulls for all images
            return imagePaths.Select(_ => (string)null).ToList();
        }
        return results;
    }
}