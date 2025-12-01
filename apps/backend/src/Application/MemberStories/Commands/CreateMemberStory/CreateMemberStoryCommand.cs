using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common;

namespace backend.Application.MemberStories.Commands.CreateMemberStory; // Updated

public record CreateMemberStoryCommand : IRequest<Result<Guid>> // Updated
{
    public Guid MemberId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Story { get; init; } = string.Empty;
    public string? OriginalImageUrl { get; init; }
    public string? ResizedImageUrl { get; init; }
    public string? RawInput { get; init; } // NEW
    public string? StoryStyle { get; init; } // NEW
    public string? Perspective { get; init; } // NEW
    public List<DetectedFaceDto> DetectedFaces { get; init; } = new List<DetectedFaceDto>();
}
