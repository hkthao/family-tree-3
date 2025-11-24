using backend.Application.Common.Models;
using backend.Application.Memories.DTOs;

namespace backend.Application.Memories.Queries.GetMemoriesByMemberId;

public record GetMemoriesByMemberIdQuery(Guid MemberId, int PageNumber = 1, int PageSize = 10) : IRequest<Result<PaginatedList<MemoryDto>>>;
