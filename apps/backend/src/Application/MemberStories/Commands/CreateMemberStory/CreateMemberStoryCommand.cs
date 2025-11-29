using backend.Application.Common.Models;
using backend.Application.Faces.Queries;

namespace backend.Application.MemberStories.Commands.CreateMemberStory; // Updated

public record CreateMemberStoryCommand : IRequest<Result<Guid>> // Updated
{
    public Guid MemberId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Story { get; init; } = string.Empty;
    public string? PhotoUrl { get; init; }
    public string? OriginalImageUrl { get; init; }
    public string? ResizedImageUrl { get; init; }
    public List<DetectedFaceDto> DetectedFaces { get; init; } = new List<DetectedFaceDto>();
}
