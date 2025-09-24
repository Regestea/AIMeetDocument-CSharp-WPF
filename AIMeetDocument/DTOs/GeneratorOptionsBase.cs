using AIMeetDocument.Enums;

namespace AIMeetDocument.DTOs;

public class GeneratorOptionsBase
{
    public string Subject { get; set; } = string.Empty;
    
    public string FileLanguage { get; set; } = "en";
    public string UserPrompt { get; set; } = string.Empty;
    
    public string OutputLanguage { get; set; } = "en";
    public FileType FileType { get; set; } = FileType.MD;
    
    public string OutputLocation { get; set; } = string.Empty;
    
    public ContentStyle ContentStyle { get; set; } = ContentStyle.None;
    
    public ContentDetails ContentDetails { get; set; } = ContentDetails.Regular;
    
    public FontOptions FontOptions { get; set; } = FontOptions.CreateDefaults();
}