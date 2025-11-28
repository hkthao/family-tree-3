using backend.Application.AI.DTOs; // For PhotoAnalysisPersonDto
using backend.Application.Common.Models;
using backend.Application.MemberStories.DTOs; // GenerateStoryResponseDto // Updated

namespace backend.Application.MemberStories.Commands.GenerateStory; // Updated

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
