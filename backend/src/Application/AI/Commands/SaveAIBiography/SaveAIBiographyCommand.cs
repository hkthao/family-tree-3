using MediatR;
using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.AI.Commands.SaveAIBiography;

public record SaveAIBiographyCommand : IRequest<Result<Guid>>
{
    public Guid MemberId { get; init; }
    public BiographyStyle Style { get; init; }
    public string Content { get; init; } = null!;
    public AIProviderType Provider { get; init; }
    public string UserPrompt { get; init; } = null!;
    public bool GeneratedFromDB { get; init; }
    public int TokensUsed { get; init; }
}
