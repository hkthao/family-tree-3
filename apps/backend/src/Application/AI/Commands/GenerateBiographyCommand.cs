using backend.Application.Common.Models;
using backend.Domain.Enums; // Add this for BiographyStyle

namespace backend.Application.AI.Commands;

public record GenerateBiographyCommand : IRequest<Result<string>>
{
    public Guid MemberId { get; init; }
    public BiographyStyle Style { get; init; }
    public string UserPrompt { get; init; } = string.Empty;
    public bool GeneratedFromDB { get; init; }
}
