using backend.Application.Common.Models;
using backend.Application.MemberStories.DTOs; // Updated

namespace backend.Application.MemberStories.Queries.GetMemberStoryDetail; // Updated

public record GetMemberStoryDetailQuery(Guid Id) : IRequest<Result<MemberStoryDto>>; // Updated
