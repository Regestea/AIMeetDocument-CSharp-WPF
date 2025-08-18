using AIMeetDocument.Enums;

namespace AIMeetDocument.DTOs;
public class Settings
{
    public DefaultAI DefaultAI { get; set; }
    public LLMStudioSettings LLMStudio { get; set; }
    public GeminiSettings Gemini { get; set; }
}