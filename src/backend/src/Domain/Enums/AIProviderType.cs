namespace backend.Domain.Enums;

/// <summary>
/// Defines the type of AI provider used for content generation.
/// </summary>
public enum AIProviderType
{
    None = 0,
    Gemini = 1,
    OpenAI = 2,
    LocalAI = 3,
    // Add more providers as needed
}
