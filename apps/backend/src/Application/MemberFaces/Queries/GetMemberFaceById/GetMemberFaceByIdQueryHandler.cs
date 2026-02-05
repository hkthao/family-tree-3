using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common; // For MemberFaceDto and BoundingBoxDto
using backend.Application.MemberFaces.Specifications; // Import the new specification

namespace backend.Application.MemberFaces.Queries.GetMemberFaceById;
public class GetMemberFaceByIdQueryHandler : IRequestHandler<GetMemberFaceByIdQuery, Result<MemberFaceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IAuthorizationService _authorizationService; // Inject IAuthorizationService
    private readonly IMapper _mapper; // Inject IMapper
    public GetMemberFaceByIdQueryHandler(IApplicationDbContext context, ICurrentUser currentUser, IAuthorizationService authorizationService, IMapper mapper) // Add IAuthorizationService to constructor
    {
        _context = context;
        _currentUser = currentUser;
        _authorizationService = authorizationService; // Assign
        _mapper = mapper; // Assign
    }
    public async Task<Result<MemberFaceDto>> Handle(GetMemberFaceByIdQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        var isAdmin = _authorizationService.IsAdmin(); // Use IAuthorizationService.IsAdmin()

        var spec = new MemberFaceAccessSpecification(isAdmin, currentUserId);

        var memberFace = await _context.MemberFaces
            .WithSpecification(spec) // Apply the specification
            .Include(mf => mf.Member) // Include the related Member data
            .FirstOrDefaultAsync(mf => mf.Id == request.Id, cancellationToken);

        if (memberFace == null)
        {
            return Result<MemberFaceDto>.Failure($"MemberFace with ID {request.Id} not found.", ErrorSources.NotFound);
        }

        // Authorization check is now handled by the specification
        // The previous explicit check `_authorizationService.CanAccessFamily` is no longer needed here.

        var dto = _mapper.Map<MemberFaceDto>(memberFace);
        return Result<MemberFaceDto>.Success(dto);
    }
}
