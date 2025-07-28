using AIMeetDocument.Enums;

namespace AIMeetDocument.DTOs;
public class Settings
{
    public DefaultAI DefaultAI { get; set; }
    public LLMStudioSettings LLMStudio { get; set; }
    public GeminiSettings Gemini { get; set; }
}



public class LLMStudioSettings
{
    public string ServerUrl { get; set; }
    public string Model { get; set; }
}

public class GeminiSettings
{
    public string ApiKey { get; set; }
    public string Model { get; set; }
}