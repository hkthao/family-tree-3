using backend.Application.Common.Models;
using backend.Application.Memories.DTOs;

namespace backend.Application.Memories.Commands.GenerateStory;

public record GenerateStoryCommand : IRequest<Result<GenerateStoryResponseDto>>
{
    public Guid MemberId { get; init; }
    public Guid? PhotoAnalysisId { get; init; }
    public string RawText { get; init; } = string.Empty;
    public string Style { get; init; } = string.Empty; // e.g., nostalgic|warm|formal|folk
    public int MaxWords { get; init; } = 500;
}
