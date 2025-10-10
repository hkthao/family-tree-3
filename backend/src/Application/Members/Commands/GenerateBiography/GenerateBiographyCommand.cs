using backend.Application.AI.Common;
using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.Members.Commands.GenerateBiography;

/// <summary>
/// Command to generate a biography for a member using AI.
/// </summary>
public class GenerateBiographyCommand : IRequest<Result<BiographyResultDto>>
{
    public Guid MemberId { get; set; }
    public BiographyStyle Style { get; set; }
    public bool GeneratedFromDB { get; set; } = false;
    public string? UserPrompt { get; set; }
    public string Language { get; set; } = "Vietnamese";
}
