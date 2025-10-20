
using backend.Application.Common.Models;

namespace backend.Application.Members.Commands.GenerateBiography;

public enum BiographyTone
{
    Emotional,
    Historical,
    Storytelling,
    Formal,
    Informal,
    Neutral // Keep Neutral as a fallback or default if needed
}

/// <summary>
/// Command to generate a biography for a member using AI.
/// </summary>
public class GenerateBiographyCommand : IRequest<Result<BiographyResultDto>>
{
    public Guid MemberId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public BiographyTone Tone { get; set; } = BiographyTone.Neutral;
    public bool UseSystemData { get; set; } = false;
}
