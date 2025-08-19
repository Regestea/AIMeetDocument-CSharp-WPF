using System.Collections.Generic;
using AIMeetDocument.Enums;

namespace AIMeetDocument.DTOs;

public class GeneratorOptions
{
    // Core selections
    public string AudioLanguage { get; set; } = "en";
    public string OutputLanguage { get; set; } = "en";
    public string FileType { get; set; } = "MD"; // MD, Word, PDF
    public string UserPrompt { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string AudioSubject { get; set; } = string.Empty;

    // UI adjuncts
    public string ContentStyle { get; set; } = "none"; // none, formal, informal
    public string ContentDetails { get; set; } = "Regular"; // Regular, Summary, Maximum details
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


