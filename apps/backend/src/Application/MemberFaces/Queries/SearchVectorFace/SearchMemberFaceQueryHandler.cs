using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.AI.Models;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Application.MemberFaces.Queries.SearchVectorFace;

public class SearchMemberFaceQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IN8nService n8nService, ILogger<SearchMemberFaceQueryHandler> logger) : IRequestHandler<SearchMemberFaceQuery, Result<List<FoundFaceDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IN8nService _n8nService = n8nService;
    private readonly ILogger<SearchMemberFaceQueryHandler> _logger = logger;

    public async Task<Result<List<FoundFaceDto>>> Handle(SearchMemberFaceQuery request, CancellationToken cancellationToken)
    {
        // If AccessibleFamilyIds are provided, it means authorization has already been applied upstream.
        // If FamilyId is explicitly provided, ensure it's in the accessible list or user is authorized.
        if (request.FamilyId.HasValue)
        {
            if (request.AccessibleFamilyIds != null && request.AccessibleFamilyIds.Any() && !request.AccessibleFamilyIds.Contains(request.FamilyId.Value))
            {
                return Result<List<FoundFaceDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
            }
            else if (request.AccessibleFamilyIds == null || !request.AccessibleFamilyIds.Any()) // No AccessibleFamilyIds provided, so check with IAuthorizationService
            {
                 if (!_authorizationService.CanAccessFamily(request.FamilyId.Value))
                 {
                    return Result<List<FoundFaceDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
                 }
            }
        }
        else if (request.AccessibleFamilyIds == null || !request.AccessibleFamilyIds.Any())
        {
            // If no FamilyId or AccessibleFamilyIds are provided, it implies the user should not see any results
            // from unauthorized families. This scenario should ideally be handled upstream by passing AccessibleFamilyIds.
            // For now, if no family context is given, we assume no access.
            // This is a safety net. In real-world, we'd fetch accessible family IDs here if not passed.
            return Result<List<FoundFaceDto>>.Success(new List<FoundFaceDto>());
        }

        if (request.Vector == null || !request.Vector.Any())
        {
            return Result<List<FoundFaceDto>>.Failure("Search vector cannot be empty.", ErrorSources.Validation);
        }

        var searchFaceVectorDto = new SearchFaceVectorOperationDto
        {
            Vector = request.Vector.Select(d => (float)d).ToList(),
            Limit = request.Limit,
            Threshold = request.Threshold,
            Filter = new Dictionary<string, object>(),
            ReturnFields = new List<string> { "localDbId", "memberId", "faceId", "thumbnailUrl", "originalImageUrl", "emotion", "emotionConfidence" }
        };

        // Apply FamilyId filter from request
        if (request.FamilyId.HasValue)
        {
            searchFaceVectorDto.Filter.Add("familyId", request.FamilyId.Value.ToString());
        }
        // Apply AccessibleFamilyIds filter if provided
        else if (request.AccessibleFamilyIds != null && request.AccessibleFamilyIds.Any())
        {
            searchFaceVectorDto.Filter.Add("familyId", request.AccessibleFamilyIds.Select(id => id.ToString()).ToList());
        }

        if (request.MemberId.HasValue)
        {
            searchFaceVectorDto.Filter.Add("memberId", request.MemberId.Value.ToString());
        }

        var n8nResult = await _n8nService.CallSearchFaceVectorWebhookAsync(searchFaceVectorDto, cancellationToken);

        if (!n8nResult.IsSuccess || n8nResult.Value == null || !n8nResult.Value.Success)
        {
            _logger.LogError("Failed to search face vectors in n8n: {Error}", n8nResult.Error ?? n8nResult.Value?.Message);
            return Result<List<FoundFaceDto>>.Failure($"Failed to search face vectors: {n8nResult.Error ?? n8nResult.Value?.Message}", ErrorSources.ExternalServiceError);
        }

        var foundFaces = new List<FoundFaceDto>();
        var memberIds = new HashSet<Guid>();

        if (n8nResult.Value.SearchResults != null)
        {
            foreach (var searchResult in n8nResult.Value.SearchResults)
            {
                if (searchResult.Payload == null) continue;

                var localDbId = Guid.Parse(searchResult.Id);

                if (!searchResult.Payload.TryGetValue("memberId", out var memberIdObj) || !Guid.TryParse(memberIdObj?.ToString(), out var memberId))
                {
                    _logger.LogWarning("memberId missing or invalid in search result payload for vector {VectorId}.", searchResult.Id);
                    continue;
                }

                var foundFaceDto = new FoundFaceDto
                {
                    MemberFaceId = localDbId,
                    MemberId = memberId,
                    FaceId = searchResult.Id.ToString(),
                    Score = searchResult.Score,
                    ThumbnailUrl = searchResult.Payload.TryGetValue("thumbnailUrl", out var thumbnailUrlObj) ? thumbnailUrlObj?.ToString() : null,
                    OriginalImageUrl = searchResult.Payload.TryGetValue("originalImageUrl", out var originalImageUrlObj) ? originalImageUrlObj?.ToString() : null,
                    Emotion = searchResult.Payload.TryGetValue("emotion", out var emotionObj) ? emotionObj?.ToString() : null,
                    EmotionConfidence = searchResult.Payload.TryGetValue("emotionConfidence", out var emotionConfidenceObj) && double.TryParse(emotionConfidenceObj?.ToString(), out var emotionConfidence) ? emotionConfidence : 0
                };
                foundFaces.Add(foundFaceDto);
                memberIds.Add(memberId);
            }
        }

        if (memberIds.Any())
        {
            var membersQuery = _context.Members
                .Where(m => memberIds.Contains(m.Id)); // Start with basic filter
            
            // If AccessibleFamilyIds were provided, filter by them
            if (request.AccessibleFamilyIds != null && request.AccessibleFamilyIds.Any())
            {
                membersQuery = membersQuery.Where(m => request.AccessibleFamilyIds.Contains(m.FamilyId));
            }

            // Now, apply the Include and then select/tolist
            var members = await membersQuery
                .Include(m => m.Family) // Apply Include here, after all Where clauses
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
