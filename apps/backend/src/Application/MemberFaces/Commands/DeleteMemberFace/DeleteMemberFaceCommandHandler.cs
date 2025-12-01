using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Events.MemberFaces; // Add this line
using Microsoft.Extensions.Logging;

namespace backend.Application.MemberFaces.Commands.DeleteMemberFace;

public class DeleteMemberFaceCommandHandler : IRequestHandler<DeleteMemberFaceCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<DeleteMemberFaceCommandHandler> _logger;

    public DeleteMemberFaceCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ILogger<DeleteMemberFaceCommandHandler> logger)
    {
        _context = context;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteMemberFaceCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MemberFaces
            .Include(mf => mf.Member) // Include Member to get FamilyId for authorization
            .FirstOrDefaultAsync(mf => mf.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<Unit>.Failure($"MemberFace with ID {request.Id} not found.", ErrorSources.NotFound);
        }

        // Authorization: Check if user can access the family this memberFace belongs to
        if (entity.Member == null || !_authorizationService.CanAccessFamily(entity.Member.FamilyId))
        {
            return Result<Unit>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        _context.MemberFaces.Remove(entity);
        entity.AddDomainEvent(new MemberFaceDeletedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted MemberFace {MemberFaceId}.", entity.Id);

        return Result<Unit>.Success(Unit.Value);
    }
}
