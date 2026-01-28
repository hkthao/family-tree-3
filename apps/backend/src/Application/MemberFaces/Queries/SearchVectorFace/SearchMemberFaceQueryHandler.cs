using Ardalis.Specification.EntityFrameworkCore;
// using backend.Application.AI.Models; // Removed
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Knowledge; // Added for IKnowledgeService
using backend.Application.MemberFaces.Common;
using Microsoft.Extensions.Logging;

namespace backend.Application.MemberFaces.Queries.SearchVectorFace;

public class SearchMemberFaceQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IKnowledgeService knowledgeService, IFaceApiService faceApiService, ILogger<SearchMemberFaceQueryHandler> logger) : IRequestHandler<SearchMemberFaceQuery, Result<List<FoundFaceDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IKnowledgeService _knowledgeService = knowledgeService;
    private readonly IFaceApiService _faceApiService = faceApiService;
    private readonly ILogger<SearchMemberFaceQueryHandler> _logger = logger;

    public async Task<Result<List<FoundFaceDto>>> Handle(SearchMemberFaceQuery request, CancellationToken cancellationToken)
    {
        if (request.Vector == null || request.Vector.Count == 0)
        {
            return Result<List<FoundFaceDto>>.Failure("Search vector cannot be empty.", ErrorSources.Validation);
        }

        // Call Face API service to search faces
        var searchRequest = new FaceSearchRequestDto
        {
            Embedding = request.Vector,
            FamilyId = request.FamilyId.ToString(),
            MemberId = request.MemberId?.ToString(),
            TopK = request.Limit
        };

        var searchResults = await _faceApiService.SearchFacesAsync(searchRequest);
        if (searchResults == null || searchResults.Count == 0)
        {
            _logger.LogInformation("No face vectors found for FamilyId: {FamilyId}", request.FamilyId);
            return Result<List<FoundFaceDto>>.Success([]);
        }

        var foundFaces = new List<FoundFaceDto>();
        var memberIds = new HashSet<Guid>();

        foreach (var searchResult in searchResults)
        {
            _logger.LogWarning("Score: {Score}.", searchResult.Score);
            // The FaceSearchResultDto from FaceApiService contains VectorDbId and Score.
            // We need to fetch the full MemberFace entity to get localDbId, thumbnailUrl, etc.

            // First, check if a MemberFace entity exists with this VectorDbId
            var memberFace = await _context.MemberFaces.AsNoTracking()
                .FirstOrDefaultAsync(mf => mf.VectorDbId == searchResult.Id, cancellationToken);

            if (memberFace == null)
            {
                _logger.LogWarning("Local MemberFace not found for VectorDbId {VectorDbId} returned from face API service search.", searchResult.Id);
                continue;
            }

            var foundFaceDto = new FoundFaceDto
            {
                MemberFaceId = memberFace.Id, // Local DB ID
                MemberId = memberFace.MemberId,
                FaceId = memberFace.FaceId, // FaceId from the local MemberFace entity
                Score = (float)searchResult.Score, // Explicit cast from double to float
                ThumbnailUrl = memberFace.ThumbnailUrl,
                OriginalImageUrl = memberFace.OriginalImageUrl,
                Emotion = memberFace.Emotion,
                EmotionConfidence = memberFace.EmotionConfidence
            };
            foundFaces.Add(foundFaceDto);
            memberIds.Add(memberFace.MemberId); // Collect member IDs for further enrichment
        }

        if (memberIds.Count != 0)
        {
            // Now, enrich with MemberName and FamilyAvatarUrl from local database
            var membersQuery = _context.Members
                .Where(m => memberIds.Contains(m.Id));

            var members = await membersQuery
                .Include(m => m.Family)
                .Select(m => new { m.Id, m.FirstName, m.LastName, m.Family, m.FamilyId })
                .ToListAsync(cancellationToken);

            foreach (var foundFace in foundFaces)
            {
                var member = members.FirstOrDefault(m => m.Id == foundFace.MemberId);
                if (member != null)
                {
                    foundFace.MemberName = $"{member.FirstName} {member.LastName}".Trim();
                    foundFace.FamilyAvatarUrl = member.Family?.AvatarUrl;
                }
            }
        }

        return Result<List<FoundFaceDto>>.Success(foundFaces);
    }
}
