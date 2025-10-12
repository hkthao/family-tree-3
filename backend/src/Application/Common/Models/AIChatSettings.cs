
namespace backend.Application.Common.Models;

public class AIChatSettings 
{
    public const string SectionName = "AIChatSettings";
    public GeminiSettings Gemini { get; set; } = new();
    public LocalAISettings Local { get; set; } = new();
    public OpenAISettings OpenAI { get; set; } = new();
}

public class GeminiSettings
{
    public string ApiKey { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string Region { get; set; } = null!;
}

public class LocalAISettings
{
    public string ApiUrl { get; set; } = "http://localhost:11434/api/chat"; // Default Ollama chat API URL
    public string Model { get; set; } = "llama2"; // Default Ollama chat model
}

public class OpenAISettings
{
    public string ApiKey { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string Organization { get; set; } = null!;
}
