using backend.Application.Common.Models;

namespace backend.Application.Memories.Commands.CreateMemory;

public record CreateMemoryCommand : IRequest<Result<Guid>>
{
    public Guid MemberId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Story { get; init; } = string.Empty;
    public Guid? PhotoAnalysisId { get; init; }
    public string? PhotoUrl { get; init; }
    public string[] Tags { get; init; } = Array.Empty<string>();
}
