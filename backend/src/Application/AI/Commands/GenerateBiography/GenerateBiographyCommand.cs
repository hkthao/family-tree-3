using MediatR;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using backend.Application.AI.Common;

namespace backend.Application.AI.Commands.GenerateBiography;

/// <summary>
/// Command to generate a biography for a member using AI.
/// </summary>
public record GenerateBiographyCommand : IRequest<Result<BiographyResultDto>>
{
    public Guid MemberId { get; init; }
    public BiographyStyle Style { get; init; }
    public bool UseDBData { get; init; } = false;
    public string? UserPrompt { get; init; }
    public string Language { get; init; } = "Vietnamese";
}
