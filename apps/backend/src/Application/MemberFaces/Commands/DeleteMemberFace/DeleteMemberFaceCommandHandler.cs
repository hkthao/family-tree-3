using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Events.MemberFaces; // Add this line
using Microsoft.Extensions.Logging;
using backend.Application.Knowledge;

namespace backend.Application.MemberFaces.Commands.DeleteMemberFace;

public class DeleteMemberFaceCommandHandler : IRequestHandler<DeleteMemberFaceCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<DeleteMemberFaceCommandHandler> _logger;
    private readonly IKnowledgeService _knowledgeService;

    public DeleteMemberFaceCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ILogger<DeleteMemberFaceCommandHandler> logger, IKnowledgeService knowledgeService)
    {
        _context = context;
        _authorizationService = authorizationService;
        _logger = logger;
        _knowledgeService = knowledgeService;
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

        // If the member face was synced to the vector DB, delete it from there first
        // The service DeleteMemberFaceData expects familyId and faceId (which is MemberFace.Id)
        if (entity.Member != null) // Ensure Member is loaded
        {
            try
            {
                await _knowledgeService.DeleteMemberFaceData(entity.Member.FamilyId, entity.Id);
                _logger.LogInformation("Successfully deleted MemberFace {MemberFaceId} from knowledge-search-service for Family {FamilyId}.", entity.Id, entity.Member.FamilyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete MemberFace {MemberFaceId} from knowledge-search-service for Family {FamilyId}. Proceeding with local deletion.", entity.Id, entity.Member.FamilyId);
                // Do not re-throw; proceed with local deletion even if knowledge service fails
            }
        }
        else
        {
            _logger.LogWarning("Member navigation property is null for MemberFace {MemberFaceId}. Cannot delete from knowledge-search-service.", entity.Id);
        }

        _context.MemberFaces.Remove(entity);
        entity.AddDomainEvent(new MemberFaceDeletedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted MemberFace {MemberFaceId}.", entity.Id);

        return Result<Unit>.Success(Unit.Value);
    }
}
