using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces; // Ensure this is present for IAuthorizationService and ICurrentUser
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common; // For MemberFaceDto and BoundingBoxDto
using backend.Application.MemberFaces.Specifications; // Import the new specification

namespace backend.Application.MemberFaces.Queries.GetMemberFaceById;
public class GetMemberFaceByIdQueryHandler : IRequestHandler<GetMemberFaceByIdQuery, Result<MemberFaceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IAuthorizationService _authorizationService; // Inject IAuthorizationService
    public GetMemberFaceByIdQueryHandler(IApplicationDbContext context, ICurrentUser currentUser, IAuthorizationService authorizationService) // Add IAuthorizationService to constructor
    {
        _context = context;
        _currentUser = currentUser;
        _authorizationService = authorizationService; // Assign
    }
    public async Task<Result<MemberFaceDto>> Handle(GetMemberFaceByIdQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        var isAdmin = _authorizationService.IsAdmin(); // Use IAuthorizationService.IsAdmin()

        var spec = new MemberFaceAccessSpecification(isAdmin, currentUserId);

        var memberFace = await _context.MemberFaces
            .WithSpecification(spec) // Apply the specification
            .FirstOrDefaultAsync(mf => mf.Id == request.Id, cancellationToken);

        if (memberFace == null)
        {
            return Result<MemberFaceDto>.Failure($"MemberFace with ID {request.Id} not found.", ErrorSources.NotFound);
        }

        // Authorization check is now handled by the specification
        // The previous explicit check `_authorizationService.CanAccessFamily` is no longer needed here.

        var dto = new MemberFaceDto
        {
            Id = memberFace.Id,
            MemberId = memberFace.MemberId,
            FaceId = memberFace.FaceId,
            BoundingBox = new MemberFaces.Common.BoundingBoxDto
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
            MemberGender = memberFace.Member?.Gender,
            MemberAvatarUrl = memberFace.Member?.AvatarUrl,
            FamilyId = memberFace.Member?.FamilyId,
            FamilyName = memberFace.Member?.Family?.Name,
            FamilyAvatarUrl = memberFace.Member?.Family?.AvatarUrl
        };
        return Result<MemberFaceDto>.Success(dto);
    }
}
