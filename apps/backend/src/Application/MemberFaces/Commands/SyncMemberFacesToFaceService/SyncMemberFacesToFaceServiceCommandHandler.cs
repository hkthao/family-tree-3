using backend.Application.Common.Constants; // Added for ErrorMessages and ErrorSources
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Interfaces.Services;
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Application.MemberFaces.Commands.SyncMemberFacesToFaceService;

public class SyncMemberFacesToFaceServiceCommandHandler(
    IApplicationDbContext context,
    IFaceApiService faceApiService,
    ILogger<SyncMemberFacesToFaceServiceCommandHandler> logger,
    IAuthorizationService authorizationService)
    : IRequestHandler<SyncMemberFacesToFaceServiceCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IFaceApiService _faceApiService = faceApiService;
    private readonly ILogger<SyncMemberFacesToFaceServiceCommandHandler> _logger = logger;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<Unit>> Handle(SyncMemberFacesToFaceServiceCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.IsAdmin())
        {
            return Result<Unit>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        IQueryable<MemberFace> query = _context.MemberFaces
            .Include(mf => mf.Member);

        if (request.FamilyId.HasValue)
        {
            var existingMemberFacesInFamily = await _context.MemberFaces
                .Where(mf => mf.Member != null && mf.Member.FamilyId == request.FamilyId.Value && mf.VectorDbId != null)
                .Select(mf => mf.VectorDbId!)
                .ToListAsync(cancellationToken);

            foreach (var vectorDbId in existingMemberFacesInFamily)
            {
                try
                {
                    await _faceApiService.DeleteFaceByIdAsync(vectorDbId);
                    _logger.LogInformation("Deleted existing face with VectorDbId {VectorDbId} for FamilyId {FamilyId} from face API service before synchronization.", vectorDbId, request.FamilyId.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete face with VectorDbId {VectorDbId} for FamilyId {FamilyId} from face API service during synchronization cleanup.", vectorDbId, request.FamilyId.Value);
                }
            }
            query = query.Where(mf => mf.Member != null && mf.Member.FamilyId == request.FamilyId.Value);
        }



        var memberFacesToSync = await query.ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} member faces to synchronize with face service.",
            memberFacesToSync.Count);

        foreach (var memberFace in memberFacesToSync)
        {
            if (memberFace.Member == null)
            {
                _logger.LogWarning("MemberFace {MemberFaceId} has no associated Member. Skipping synchronization.", memberFace.Id);
                continue;
            }

            var faceAddVectorRequest = new FaceAddVectorRequestDto
            {
                Vector = memberFace.Embedding, // Embedding goes to Vector property
                Metadata = new FaceMetadataDto
                {
                    FamilyId = memberFace.Member.FamilyId.ToString(),
                    MemberId = memberFace.MemberId.ToString(),
                    FaceId = memberFace.Id.ToString(), // Local DB ID for the member face
                    BoundingBox = new BoundingBoxDto
                    {
                        X = memberFace.BoundingBox.X,
                        Y = memberFace.BoundingBox.Y,
                        Width = memberFace.BoundingBox.Width,
                        Height = memberFace.BoundingBox.Height
                    },
                    Confidence = (double)memberFace.Confidence,
                    ThumbnailUrl = memberFace.ThumbnailUrl,
                    OriginalImageUrl = memberFace.OriginalImageUrl,
                    Emotion = memberFace.Emotion,
                    EmotionConfidence = (double)memberFace.EmotionConfidence
                }
            };

            try
            {
                var result = await _faceApiService.AddFaceByVectorAsync(faceAddVectorRequest);

                if (result != null && result.TryGetValue("vector_db_id", out var vectorDbIdObject) && vectorDbIdObject is string returnedVectorDbId)
                {
                    memberFace.MarkAsVectorDbSynced(returnedVectorDbId);
                    _logger.LogInformation("Successfully synchronized MemberFace {MemberFaceId} to face API service with VectorDbId {VectorDbId}.",
                        memberFace.Id, returnedVectorDbId);
                }
                else
                {
                    memberFace.IsVectorDbSynced = false;
                    _logger.LogError("Face API service returned no VectorDbId for MemberFace {MemberFaceId}.", memberFace.Id);
                }
            }
            catch (Exception ex)
            {
                memberFace.IsVectorDbSynced = false;
                _logger.LogError(ex, "Failed to synchronize MemberFace {MemberFaceId} with face service.", memberFace.Id);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
