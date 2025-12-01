using System.Linq.Expressions;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common; // For MemberFaceDto and BoundingBoxDto
using backend.Domain.Entities;
namespace backend.Application.MemberFaces.Queries.SearchMemberFaces;
public class SearchMemberFacesQueryHandler : IRequestHandler<SearchMemberFacesQuery, Result<PaginatedList<MemberFaceDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    public SearchMemberFacesQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }
    public async Task<Result<PaginatedList<MemberFaceDto>>> Handle(SearchMemberFacesQuery request, CancellationToken cancellationToken)
    {
        IQueryable<MemberFace> query = _context.MemberFaces
            .Include(mf => mf.Member) // Include Member to get FamilyId
            .ThenInclude(m => m!.Family); // Then include Family to get FamilyName
        // Filter by FamilyId
        if (request.FamilyId.HasValue)
        {
            query = query.Where(mf => mf.Member != null && mf.Member.FamilyId == request.FamilyId.Value);
        }
        // Filter by MemberId
        if (request.MemberId.HasValue)
        {
            query = query.Where(mf => mf.MemberId == request.MemberId.Value);
        }
        // Filter by Emotion
        if (!string.IsNullOrWhiteSpace(request.Emotion))
        {
            query = query.Where(mf => mf.Emotion != null && mf.Emotion.ToLower() == request.Emotion.ToLower());
        }
        // Filter by SearchQuery (on MemberName or Emotion)
        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            query = query.Where(mf => (mf.Member != null && (mf.Member.FirstName.ToLower().Contains(request.SearchQuery.ToLower()) || mf.Member.LastName.ToLower().Contains(request.SearchQuery.ToLower()))) || (mf.Emotion != null && mf.Emotion.ToLower().Contains(request.SearchQuery.ToLower())));
        }
        // TODO: Implement proper authorization for listing multiple faces.
        // For now, if a FamilyId is provided, authorize access to that family.
        if (request.FamilyId.HasValue && !_authorizationService.CanAccessFamily(request.FamilyId.Value))
        {
            return Result<PaginatedList<MemberFaceDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }
        // If no FamilyId is provided, we would need to filter by all families the user can access.
        // As GetAccessibleFamilyIds is not available, this is left as a TODO for now.
        if (!await query.AnyAsync(cancellationToken)) // Use AnyAsync here
        {
            return Result<PaginatedList<MemberFaceDto>>.Success(new PaginatedList<MemberFaceDto>(new List<MemberFaceDto>(), 0, request.Page, request.ItemsPerPage));
        }
        // Sorting
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            var sortOrder = request.SortOrder?.ToLowerInvariant();
            if (request.SortBy.Equals("membername", StringComparison.OrdinalIgnoreCase))
            {
                if (sortOrder == "asc")
                {
                    query = query.OrderBy(mf => mf.Member!.LastName)
                                 .ThenBy(mf => mf.Member.FirstName);
                }
                else
                {
                    query = query.OrderByDescending(mf => mf.Member!.LastName)
                                 .ThenByDescending(mf => mf.Member.FirstName);
                }
            }
            else if (request.SortBy.Equals("familyname", StringComparison.OrdinalIgnoreCase))
            {
                if (sortOrder == "asc")
                {
                    query = query.OrderBy(mf => mf.Member!.Family!.Name);
                }
                else
                {
                    query = query.OrderByDescending(mf => mf.Member!.Family!.Name);
                }
            }
            else
            {
                // Fallback to dynamic expression building for other properties
                Expression<Func<MemberFace, object>> orderByExpression = request.SortBy.ToLowerInvariant() switch
                {
                    "faceid" => mf => mf.FaceId,
                    "confidence" => mf => mf.Confidence,
                    _ => mf => mf.Created // Default sort by creation date
                };

                if (sortOrder == "asc")
                {
                    query = query.OrderBy(orderByExpression);
                }
                else
                {
                    query = query.OrderByDescending(orderByExpression);
                }
            }
        }
        else
        {
            // Default sorting if no SortBy is specified
            query = query.OrderByDescending(mf => mf.Created);
        }
        var paginatedList = await PaginatedList<MemberFaceDto>.CreateAsync(
            query.Select(mf => new MemberFaceDto
            {
                Id = mf.Id,
                MemberId = mf.MemberId,
                FaceId = mf.FaceId,
                BoundingBox = new BoundingBoxDto
                {
                    X = (int)mf.BoundingBox.X,
                    Y = (int)mf.BoundingBox.Y,
                    Width = (int)mf.BoundingBox.Width,
                    Height = (int)mf.BoundingBox.Height
                },
                Confidence = mf.Confidence,
                ThumbnailUrl = mf.ThumbnailUrl,
                OriginalImageUrl = mf.OriginalImageUrl,
                Embedding = mf.Embedding,
                Emotion = mf.Emotion,
                EmotionConfidence = mf.EmotionConfidence,
                IsVectorDbSynced = mf.IsVectorDbSynced,
                VectorDbId = mf.VectorDbId,
                MemberName = mf.Member!.LastName + " " + mf.Member.FirstName,
                MemberGender = mf.Member!.Gender, // NEW
                MemberAvatarUrl = mf.Member!.AvatarUrl, // NEW
                FamilyId = mf.Member!.FamilyId,
                FamilyName = mf.Member!.Family!.Name,
                FamilyAvatarUrl = mf.Member!.Family!.AvatarUrl
            }).AsNoTracking(),
            request.Page,
            request.ItemsPerPage
        );
        return Result<PaginatedList<MemberFaceDto>>.Success(paginatedList);
    }
}
