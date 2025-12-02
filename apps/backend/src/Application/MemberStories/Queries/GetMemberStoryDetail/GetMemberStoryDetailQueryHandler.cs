using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemberStories.DTOs; // Updated

namespace backend.Application.MemberStories.Queries.GetMemberStoryDetail; // Updated

public class GetMemberStoryDetailQueryHandler : IRequestHandler<GetMemberStoryDetailQuery, Result<MemberStoryDto>> // Updated
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUser _currentUser; // Added

    public GetMemberStoryDetailQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser) // Modified
    {
        _context = context;
        _mapper = mapper;
        _authorizationService = authorizationService;
        _currentUser = currentUser; // Added
    }

    public async Task<Result<MemberStoryDto>> Handle(GetMemberStoryDetailQuery request, CancellationToken cancellationToken) // Updated
    {
        var memberStory = await _context.MemberStories // Updated
            .Include(m => m.Member)
            .FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted, cancellationToken);

        if (memberStory == null)
        {
            return Result<MemberStoryDto>.Failure(string.Format(ErrorMessages.NotFound, $"MemberStory with ID {request.Id}"), ErrorSources.NotFound); // Updated
        }

        // Authorization check
        if (!_authorizationService.CanAccessFamily(memberStory.Member.FamilyId))
        {
            return Result<MemberStoryDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden); // Updated
        }

        var memberStoryDto = _mapper.Map<MemberStoryDto>(memberStory); // Updated
        return Result<MemberStoryDto>.Success(memberStoryDto); // Updated
    }
}
