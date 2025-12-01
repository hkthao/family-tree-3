using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemberStories.DTOs; // Updated

namespace backend.Application.MemberStories.Queries.GetMemoriesByMemberId; // Updated

public class GetMemberStoriesByMemberIdQueryHandler : IRequestHandler<GetMemberStoriesByMemberIdQuery, Result<PaginatedList<MemberStoryDto>>> // Updated
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;

    public GetMemberStoriesByMemberIdQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService)
    {
        _context = context;
        _mapper = mapper;
        _authorizationService = authorizationService;
    }

    public async Task<Result<PaginatedList<MemberStoryDto>>> Handle(GetMemberStoriesByMemberIdQuery request, CancellationToken cancellationToken) // Updated
    {
        // Find the member to ensure it exists and belong to the family
        var member = await _context.Members.FindAsync(new object[] { request.MemberId }, cancellationToken);
        if (member == null)
        {
            return Result<PaginatedList<MemberStoryDto>>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId}"), ErrorSources.NotFound); // Updated
        }

        // Authorization check
        if (!_authorizationService.CanAccessFamily(member.FamilyId))
        {
            return Result<PaginatedList<MemberStoryDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden); // Updated
        }

        var query = _context.MemberStories // Updated
            .Where(m => m.MemberId == request.MemberId && !m.IsDeleted)
            .OrderByDescending(m => m.Created) // Default sort (Changed from CreatedAt to Created)
            .AsNoTracking();

        var paginatedList = await PaginatedList<MemberStoryDto>.CreateAsync( // Updated
            query.ProjectTo<MemberStoryDto>(_mapper.ConfigurationProvider).AsQueryable(), // Updated
            request.PageNumber,
            request.PageSize
        );

        return Result<PaginatedList<MemberStoryDto>>.Success(paginatedList); // Updated
    }
}
