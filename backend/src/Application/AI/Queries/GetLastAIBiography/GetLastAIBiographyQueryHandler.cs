using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.AI.Common;
using backend.Application.AI.Specifications;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.AI.Queries.GetLastAIBiography;

public class GetLastAIBiographyQueryHandler : IRequestHandler<GetLastAIBiographyQuery, Result<AIBiographyDto?>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMapper _mapper;

    public GetLastAIBiographyQueryHandler(IApplicationDbContext context, IUser user, IAuthorizationService authorizationService, IMapper mapper)
    {
        _context = context;
        _user = user;
        _authorizationService = authorizationService;
        _mapper = mapper;
    }

    public async Task<Result<AIBiographyDto?>> Handle(GetLastAIBiographyQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _user.Id;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Result<AIBiographyDto?>.Failure("User is not authenticated.", "Authentication");
        }

        var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
        if (currentUserProfile == null)
        {
            return Result<AIBiographyDto?>.Failure("User profile not found.", "NotFound");
        }

        var member = await _context.Members.FindAsync(new object[] { request.MemberId }, cancellationToken);
        if (member == null)
        {
            return Result<AIBiographyDto?>.Failure($"Member with ID {request.MemberId} not found.", "NotFound");
        }

        if (!_authorizationService.IsAdmin() && !_authorizationService.CanManageFamily(member.FamilyId, currentUserProfile))
        {
            return Result<AIBiographyDto?>.Failure("Access denied. Only family managers or admins can view the AI biography.", "Forbidden");
        }

        var spec = new LastAIBiographyByMemberIdSpec(request.MemberId);

        var lastBiography = await _context.AIBiographies
            .WithSpecification(spec)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastBiography == null)
        {
            return Result<AIBiographyDto?>.Success(null);
        }

        return Result<AIBiographyDto?>.Success(_mapper.Map<AIBiographyDto>(lastBiography));
    }
}
