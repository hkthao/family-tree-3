using backend.Application.Common.Models;

namespace backend.Application.MemberStories.Commands.CreateMemberStory; // Updated

public record CreateMemberStoryCommand : IRequest<Result<Guid>> // Updated
{
    public Guid MemberId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Story { get; init; } = string.Empty;
    public string? PhotoUrl { get; init; }
    public string[] Tags { get; init; } = Array.Empty<string>();
}
