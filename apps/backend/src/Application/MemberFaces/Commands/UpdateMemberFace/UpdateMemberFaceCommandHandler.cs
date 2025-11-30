using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using backend.Domain.ValueObjects; // For BoundingBox

namespace backend.Application.MemberFaces.Commands.UpdateMemberFace;

public class UpdateMemberFaceCommandHandler : IRequestHandler<UpdateMemberFaceCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<UpdateMemberFaceCommandHandler> _logger;

    public UpdateMemberFaceCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ILogger<UpdateMemberFaceCommandHandler> logger)
    {
        _context = context;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(UpdateMemberFaceCommand request, CancellationToken cancellationToken)
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

        // Update properties
        entity.MemberId = request.MemberId;
        entity.FaceId = request.FaceId;
        entity.BoundingBox = new BoundingBox {
            X = request.BoundingBox.X,
            Y = request.BoundingBox.Y,
            Width = request.BoundingBox.Width,
            Height = request.BoundingBox.Height
        };
        entity.Confidence = request.Confidence;
        entity.ThumbnailUrl = request.ThumbnailUrl;
        entity.OriginalImageUrl = request.OriginalImageUrl;
        entity.Embedding = request.Embedding;
        entity.Emotion = request.Emotion;
        entity.EmotionConfidence = request.EmotionConfidence ?? 0.0; // Handle nullable
        entity.IsVectorDbSynced = request.IsVectorDbSynced;
        entity.VectorDbId = request.VectorDbId;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated MemberFace {MemberFaceId}.", entity.Id);

        return Result<Unit>.Success(Unit.Value);
    }
}
