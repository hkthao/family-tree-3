using backend.Application.Common.Models;

namespace backend.Application.MemberStories.Commands.DeleteMemberStory; // Updated

public record DeleteMemberStoryCommand : IRequest<Result> // Updated
{
    public Guid Id { get; init; }
}
