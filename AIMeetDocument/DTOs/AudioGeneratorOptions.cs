using System.Collections.Generic;
using AIMeetDocument.Enums;

namespace AIMeetDocument.DTOs;



public class AudioGeneratorOptions : GeneratorOptionsBase
{
   
    public bool AutoFilter { get; set; } = false;

    // Model selection
    public string AudioDetectionModelPath { get; set; } = string.Empty;

    // Audio inputs
    public List<string> AudioFilePaths { get; set; } = new();
}
