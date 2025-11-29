using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.AI.Models;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Faces.Specifications;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Application.Faces.Commands.DeleteMemberFace;

public class DeleteMemberFaceCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IN8nService n8nService, ILogger<DeleteMemberFaceCommandHandler> logger) : IRequestHandler<DeleteMemberFaceCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IN8nService _n8nService = n8nService;
    private readonly ILogger<DeleteMemberFaceCommandHandler> _logger = logger;

    public async Task<Result<Unit>> Handle(DeleteMemberFaceCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve MemberFace entity
        var memberFace = await _context.MemberFaces
            .Include(mf => mf.Member)
            .FirstOrDefaultAsync(mf => mf.Id == request.MemberFaceId, cancellationToken);

        if (memberFace == null)
        {
            return Result<Unit>.Failure(string.Format(ErrorMessages.NotFound, $"MemberFace with ID {request.MemberFaceId} not found."), ErrorSources.NotFound);
        }

        // 2. Authorize
        if (memberFace.Member == null)
        {
            return Result<Unit>.Failure($"Member associated with MemberFace ID {request.MemberFaceId} not found.", ErrorSources.NotFound);
        }
        if (!_authorizationService.CanAccessFamily(memberFace.Member.FamilyId))
        {
            return Result<Unit>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // 3. Call n8n Face Vector Webhook for Delete
        if (!string.IsNullOrEmpty(memberFace.VectorDbId))
        {
            var deleteFaceVectorDto = new DeleteFaceVectorOperationDto
            {
                Filter = new Dictionary<string, object>
                {
                    { "vectorDbId", memberFace.VectorDbId }
                }
            };

            var n8nResult = await _n8nService.CallDeleteFaceVectorWebhookAsync(deleteFaceVectorDto, cancellationToken);

            if (!n8nResult.IsSuccess || n8nResult.Value == null || !n8nResult.Value.Success)
            {
                _logger.LogError("Failed to delete face vector for MemberFaceId {MemberFaceId} in n8n: {Error}", memberFace.Id, n8nResult.Error ?? n8nResult.Value?.Message);
                return Result<Unit>.Failure($"Failed to delete face vector in external DB: {n8nResult.Error ?? n8nResult.Value?.Message}", ErrorSources.ExternalServiceError);
            }
            _logger.LogInformation("Successfully deleted face vector for MemberFaceId {MemberFaceId} in n8n. VectorDbId: {VectorDbId}", memberFace.Id, memberFace.VectorDbId);
        }
        else
        {
            _logger.LogWarning("MemberFaceId {MemberFaceId} has no VectorDbId, skipping n8n delete operation.", memberFace.Id);
        }

        // 4. Remove MemberFace entity from local database
        _context.MemberFaces.Remove(memberFace);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
