using System.Collections.Generic;
using AIMeetDocument.Enums;

namespace AIMeetDocument.DTOs;

public class GeneratorOptions
{
    // Core selections
    public string AudioLanguage { get; set; } = "en";
    public string OutputLanguage { get; set; } = "en";
    public FileType FileType { get; set; } = FileType.MD; // MD, Word, PDF
    public string UserPrompt { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string AudioSubject { get; set; } = string.Empty;

    // UI adjuncts
    public ContentStyle ContentStyle { get; set; } = ContentStyle.None;
    public ContentDetails ContentDetails { get; set; } = ContentDetails.Regular;
    public bool AutoFilter { get; set; } = false;

    // Fonts
    public FontOptions FontOptions { get; set; } = FontOptions.CreateDefaults();

    // Text direction derived from output language
    public TextDirection TextDirection { get; set; } = TextDirection.LTR;

    // Model selection
    public string AudioDetectionModelPath { get; set; } = string.Empty;

    // Audio inputs
    public List<string> AudioFilePaths { get; set; } = new();
}


