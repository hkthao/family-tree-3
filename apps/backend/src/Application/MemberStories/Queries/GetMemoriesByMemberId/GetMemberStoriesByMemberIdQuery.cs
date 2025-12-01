using backend.Application.Common.Models;
using backend.Application.MemberStories.DTOs; // Updated

namespace backend.Application.MemberStories.Queries.GetMemoriesByMemberId; // Updated

public record GetMemberStoriesByMemberIdQuery(Guid MemberId, int PageNumber = 1, int PageSize = 10) : IRequest<Result<PaginatedList<MemberStoryDto>>>; // Updated
