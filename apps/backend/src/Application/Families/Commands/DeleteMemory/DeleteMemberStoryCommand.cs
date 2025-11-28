using backend.Application.Common.Models;

namespace backend.Application.MemberStories.Commands.DeleteMemberStory; // Updated

public record DeleteMemberStoryCommand(Guid Id) : IRequest<Result>; // Updated
