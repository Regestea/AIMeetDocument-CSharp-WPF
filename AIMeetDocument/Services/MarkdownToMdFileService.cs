using System.IO;

namespace AIMeetDocument.Services;

public class MarkdownToMdFileService
{
    public void SaveMarkdownToFile(string markdownContent, string outputFilePath)
    {
        File.WriteAllText(outputFilePath, markdownContent);
    }
}