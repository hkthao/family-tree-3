using backend.Application.Common.Constants; // Added for ErrorMessages and ErrorSources
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Knowledge;
using backend.Application.Knowledge.DTOs;
using backend.Domain.Entities;
using backend.Domain.ValueObjects; // Added for BoundingBox
using Microsoft.Extensions.Logging;

namespace backend.Application.MemberFaces.Commands.SyncMemberFacesToKnowledgeService;

public class SyncMemberFacesToKnowledgeServiceCommandHandler(
    IApplicationDbContext context,
    IKnowledgeService knowledgeService,
    ILogger<SyncMemberFacesToKnowledgeServiceCommandHandler> logger,
    IAuthorizationService authorizationService)
    : IRequestHandler<SyncMemberFacesToKnowledgeServiceCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IKnowledgeService _knowledgeService = knowledgeService;
    private readonly ILogger<SyncMemberFacesToKnowledgeServiceCommandHandler> _logger = logger;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<Unit>> Handle(SyncMemberFacesToKnowledgeServiceCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.IsAdmin())
        {
            return Result<Unit>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        IQueryable<MemberFace> query = _context.MemberFaces
            .Include(mf => mf.Member);

        if (request.FamilyId.HasValue)
        {
            await _knowledgeService.DeleteFamilyFacesData(request.FamilyId.Value);
            _logger.LogInformation("Deleted existing faces for FamilyId {FamilyId} from knowledge service before synchronization.", request.FamilyId.Value);
            query = query.Where(mf => mf.Member != null && mf.Member.FamilyId == request.FamilyId.Value);
        }



        var memberFacesToSync = await query.ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} member faces to synchronize with knowledge service.",
            memberFacesToSync.Count);

        foreach (var memberFace in memberFacesToSync)
        {
            if (memberFace.Member == null)
            {
                _logger.LogWarning("MemberFace {MemberFaceId} has no associated Member. Skipping synchronization.", memberFace.Id);
                continue;
            }

            var memberFaceDto = new MemberFaceDto
            {
                FamilyId = memberFace.Member.FamilyId,
                MemberId = memberFace.MemberId,
                FaceId = memberFace.Id,
                BoundingBox = new BoundingBox // Assuming BoundingBox is a ValueObject and can be directly assigned
                {
                    X = memberFace.BoundingBox.X,
                    Y = memberFace.BoundingBox.Y,
                    Width = memberFace.BoundingBox.Width,
                    Height = memberFace.BoundingBox.Height
                },
                Confidence = memberFace.Confidence,
                ThumbnailUrl = memberFace.ThumbnailUrl,
                OriginalImageUrl = memberFace.OriginalImageUrl,
                Embedding = memberFace.Embedding,
                Emotion = memberFace.Emotion,
                EmotionConfidence = memberFace.EmotionConfidence,
                IsVectorDbSynced = false, // Will be updated by service
                VectorDbId = memberFace.VectorDbId // Pass existing if any
            };

            try
            {
                string returnedVectorDbId = await _knowledgeService.IndexMemberFaceData(memberFaceDto);

                if (!string.IsNullOrEmpty(returnedVectorDbId))
                {
                    memberFace.MarkAsVectorDbSynced(returnedVectorDbId);
                    _logger.LogInformation("Successfully synchronized MemberFace {MemberFaceId} to knowledge service with VectorDbId {VectorDbId}.",
                        memberFace.Id, returnedVectorDbId);
                }
                else
                {
                    memberFace.IsVectorDbSynced = false;
                    _logger.LogError("Knowledge service returned empty VectorDbId for MemberFace {MemberFaceId}.", memberFace.Id);
                }
            }
            catch (Exception ex)
            {
                memberFace.IsVectorDbSynced = false;
                _logger.LogError(ex, "Failed to synchronize MemberFace {MemberFaceId} with knowledge service.", memberFace.Id);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
