using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.AI.Commands;

public record GenerateBiographyCommand : IRequest<Result<string>>
{
    public Guid MemberId { get; init; }
    public string Style { get; init; } = string.Empty;
    public string UserPrompt { get; init; } = string.Empty;
    public bool GeneratedFromDB { get; init; }
}
