using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using backend.Application.MemberFaces.Queries.MemberFaces; // For MemberFaceDto
using System.Linq.Expressions;

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
        Expression<Func<MemberFace, object>> orderByExpression = request.SortBy?.ToLowerInvariant() switch
        {
            "faceid" => mf => mf.FaceId,
            "confidence" => mf => mf.Confidence,
            "membername" => mf => mf.Member!.FullName, // Needs to be careful with nullables here
            "familyname" => mf => mf.Member!.Family!.Name,
            _ => mf => mf.Created // Default sort by creation date
        };

        if (request.SortOrder?.ToLowerInvariant() == "asc")
        {
            query = query.OrderBy(orderByExpression);
        }
        else
        {
            query = query.OrderByDescending(orderByExpression);
        }

        var paginatedList = await PaginatedList<MemberFaceDto>.CreateAsync(
            query.Select(mf => new MemberFaceDto
            {
                Id = mf.Id,
                MemberId = mf.MemberId,
                FaceId = mf.FaceId,
                BoundingBox = new backend.Application.Faces.Common.BoundingBoxDto
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
                MemberName = mf.Member!.FullName,
                FamilyId = mf.Member!.FamilyId,
                FamilyName = mf.Member!.Family!.Name
            }).AsNoTracking(),
            request.Page,
            request.ItemsPerPage
        );

        return Result<PaginatedList<MemberFaceDto>>.Success(paginatedList);
    }
}
