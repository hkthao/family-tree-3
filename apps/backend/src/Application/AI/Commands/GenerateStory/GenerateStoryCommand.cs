using backend.Application.Common.Models;
using backend.Application.Memories.DTOs; // GenerateStoryResponseDto
using backend.Application.AI.DTOs; // For PhotoAnalysisPersonDto

namespace backend.Application.Memories.Commands.GenerateStory;

public record GenerateStoryCommand : IRequest<Result<GenerateStoryResponseDto>>
{
    public Guid MemberId { get; init; }
    public string RawText { get; init; } = string.Empty;
    public string Style { get; init; } = string.Empty; // e.g., nostalgic|warm|formal|folk
    public string? Perspective { get; init; }
    public string? MemberName { get; init; } // Added MemberName
    public string? ResizedImageUrl { get; init; }
    public List<PhotoAnalysisPersonDto>? PhotoPersons { get; init; }
    public int MaxWords { get; init; } = 500;
}
