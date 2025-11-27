using backend.Application.Common.Models;
using backend.Application.Memories.DTOs; // GenerateStoryResponseDto
using backend.Application.AI.Commands.AnalyzePhoto;
using backend.Application.AI.DTOs; // For PhotoAnalysisPersonDto

namespace backend.Application.Memories.Commands.GenerateStory;

public record GenerateStoryCommand : IRequest<Result<GenerateStoryResponseDto>>
{
    public Guid MemberId { get; init; }
    public string RawText { get; init; } = string.Empty;
    public string Style { get; init; } = string.Empty; // e.g., nostalgic|warm|formal|folk
    public string? Perspective { get; init; }
    public string? Event { get; init; }
    public string? CustomEventDescription { get; init; }
    public List<string>? EmotionContexts { get; init; }
    public string? PhotoSummary { get; init; }
    public string? PhotoScene { get; init; }
    public string? PhotoEventAnalysis { get; init; }
    public string? PhotoEmotionAnalysis { get; init; }
    public string? PhotoYearEstimate { get; init; }
    public List<string>? PhotoObjects { get; init; }
    public List<PhotoAnalysisPersonDto>? PhotoPersons { get; init; }
    public int MaxWords { get; init; } = 500;
}
