using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using backend.Application.MemberFaces.Queries.MemberFaces; // For MemberFaceDto

namespace backend.Application.MemberFaces.Queries.GetMemberFaceById;

public class GetMemberFaceByIdQueryHandler : IRequestHandler<GetMemberFaceByIdQuery, Result<MemberFaceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public GetMemberFaceByIdQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    public async Task<Result<MemberFaceDto>> Handle(GetMemberFaceByIdQuery request, CancellationToken cancellationToken)
    {
        var memberFace = await _context.MemberFaces
            .Include(mf => mf.Member) // Include Member to get FamilyId
            .FirstOrDefaultAsync(mf => mf.Id == request.Id, cancellationToken);

        if (memberFace == null)
        {
            return Result<MemberFaceDto>.Failure($"MemberFace with ID {request.Id} not found.", ErrorSources.NotFound);
        }

        // Authorization: Check if user can access the family this memberFace belongs to
        if (memberFace.Member == null || !_authorizationService.CanAccessFamily(memberFace.Member.FamilyId))
        {
            return Result<MemberFaceDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var dto = new MemberFaceDto
        {
            Id = memberFace.Id,
            MemberId = memberFace.MemberId,
            FaceId = memberFace.FaceId,
            BoundingBox = new backend.Application.Faces.Common.BoundingBoxDto
            {
                X = (int)memberFace.BoundingBox.X,
                Y = (int)memberFace.BoundingBox.Y,
                Width = (int)memberFace.BoundingBox.Width,
                Height = (int)memberFace.BoundingBox.Height
            },
            Confidence = memberFace.Confidence,
            ThumbnailUrl = memberFace.ThumbnailUrl,
            OriginalImageUrl = memberFace.OriginalImageUrl,
            Embedding = memberFace.Embedding,
            Emotion = memberFace.Emotion,
            EmotionConfidence = memberFace.EmotionConfidence,
            IsVectorDbSynced = memberFace.IsVectorDbSynced,
            VectorDbId = memberFace.VectorDbId,
            MemberName = memberFace.Member?.FullName,
            FamilyId = memberFace.Member?.FamilyId,
            FamilyName = memberFace.Member?.Family?.Name
        };

        return Result<MemberFaceDto>.Success(dto);
    }
}
