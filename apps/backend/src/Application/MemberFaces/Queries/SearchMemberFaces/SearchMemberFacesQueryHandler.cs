using System.Linq.Expressions;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore; // Add this import
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common; // For MemberFaceDto and BoundingBoxDto
using backend.Domain.Entities;
using backend.Application.MemberFaces.Specifications; // Import the new specification

namespace backend.Application.MemberFaces.Queries.SearchMemberFaces;
public class SearchMemberFacesQueryHandler : IRequestHandler<SearchMemberFacesQuery, Result<PaginatedList<MemberFaceDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser; // Inject ICurrentUser
    private readonly IAuthorizationService _authorizationService; // Inject IAuthorizationService

    public SearchMemberFacesQueryHandler(IApplicationDbContext context, ICurrentUser currentUser, IAuthorizationService authorizationService) // Modify constructor
    {
        _context = context;
        _currentUser = currentUser;
        _authorizationService = authorizationService;
    }
    public async Task<Result<PaginatedList<MemberFaceDto>>> Handle(SearchMemberFacesQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        var isAdmin = _authorizationService.IsAdmin(); // Use IAuthorizationService.IsAdmin()

        // Apply authorization specification
        var spec = new MemberFaceAccessSpecification(isAdmin, currentUserId);
        IQueryable<MemberFace> query = _context.MemberFaces.WithSpecification(spec);

        // Existing filters (apply after authorization spec)
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

        // Remove old explicit authorization check
        // The authorization is now handled by MemberFaceAccessSpecification
        // If the query results in no accessible items, the PaginatedList will be empty.
        if (!await query.AnyAsync(cancellationToken))
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
                MemberName = mf.Member != null ? (mf.Member.LastName + " " + mf.Member.FirstName).Trim() : null, // Fix CS8072
                MemberGender = mf.Member != null ? mf.Member.Gender : null, // Fix CS8602 (Gender is already string?)
                MemberAvatarUrl = mf.Member != null ? mf.Member.AvatarUrl : null, // Fix CS8072
                FamilyId = mf.Member != null ? mf.Member.FamilyId : (Guid?)null, // Fix CS8072
                FamilyName = mf.Member != null && mf.Member.Family != null ? mf.Member.Family.Name : null, // Fix CS8072
                FamilyAvatarUrl = mf.Member != null && mf.Member.Family != null ? mf.Member.Family.AvatarUrl : null // Fix CS8072
            }).AsNoTracking(),
            request.Page,
            request.ItemsPerPage
        );
        return Result<PaginatedList<MemberFaceDto>>.Success(paginatedList);
    }
}
