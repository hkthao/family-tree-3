using backend.Application.AI.DTOs; // For PhotoAnalysisResultDto
using backend.Application.Common.Models;
using backend.Application.Memories.DTOs;

namespace backend.Application.Memories.Commands.GenerateStory;

public record GenerateStoryCommand : IRequest<Result<GenerateStoryResponseDto>>
{
    public Guid MemberId { get; init; }
    public Guid? PhotoAnalysisId { get; init; }
    public PhotoAnalysisResultDto? PhotoAnalysisResult { get; init; } // NEW
    public string RawText { get; init; } = string.Empty;
    public string Style { get; init; } = string.Empty; // e.g., nostalgic|warm|formal|folk
    public string? Perspective { get; init; }
    public string? Event { get; init; }
    public string? CustomEventDescription { get; init; }
    public List<string>? EmotionContexts { get; init; }
    public int MaxWords { get; init; } = 500;
}
