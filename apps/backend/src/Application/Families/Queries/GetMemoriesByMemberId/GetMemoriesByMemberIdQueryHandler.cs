using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Memories.DTOs;

namespace backend.Application.Memories.Queries.GetMemoriesByMemberId;

public class GetMemoriesByMemberIdQueryHandler : IRequestHandler<GetMemoriesByMemberIdQuery, Result<PaginatedList<MemoryDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;

    public GetMemoriesByMemberIdQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService)
    {
        _context = context;
        _mapper = mapper;
        _authorizationService = authorizationService;
    }

    public async Task<Result<PaginatedList<MemoryDto>>> Handle(GetMemoriesByMemberIdQuery request, CancellationToken cancellationToken)
    {
        // Find the member to ensure it exists and belong to the family
        var member = await _context.Members.FindAsync(new object[] { request.MemberId }, cancellationToken);
        if (member == null)
        {
            return Result<PaginatedList<MemoryDto>>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId}"), ErrorSources.NotFound);
        }

        // Authorization check
        if (!_authorizationService.CanAccessFamily(member.FamilyId))
        {
            return Result<PaginatedList<MemoryDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var query = _context.Memories
            .Where(m => m.MemberId == request.MemberId && !m.IsDeleted)
            .OrderByDescending(m => m.Created) // Default sort (Changed from CreatedAt to Created)
            .AsNoTracking();

        var paginatedList = await PaginatedList<MemoryDto>.CreateAsync(
            query.ProjectTo<MemoryDto>(_mapper.ConfigurationProvider).AsQueryable(),
            request.PageNumber,
            request.PageSize
        );

        return Result<PaginatedList<MemoryDto>>.Success(paginatedList);
    }
}
