using backend.Application.Common.Models;

namespace backend.Application.MemberStories.Commands.UpdateMemberStory; // Updated

public record UpdateMemberStoryCommand : IRequest<Result> // Updated
{
    public Guid Id { get; init; }
    public Guid MemberId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Story { get; init; } = string.Empty;
    public string? PhotoUrl { get; init; }
    public string[] Tags { get; init; } = Array.Empty<string>();
}
