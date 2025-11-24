using backend.Application.Common.Models;

namespace backend.Application.Memories.Commands.UpdateMemory;

public record UpdateMemoryCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public Guid MemberId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Story { get; init; } = string.Empty;
    public Guid? PhotoAnalysisId { get; init; }
    public string? PhotoUrl { get; init; }
    public string[] Tags { get; init; } = Array.Empty<string>();
}
