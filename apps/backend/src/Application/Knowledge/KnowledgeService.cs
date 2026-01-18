using backend.Application.Common.Interfaces;
using backend.Application.Knowledge.DTOs;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using backend.Application.Common.Models.AppSetting;
using Microsoft.Extensions.Options;
using backend.Domain.Enums; // Added

namespace backend.Application.Knowledge;

public class KnowledgeService : IKnowledgeService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<KnowledgeService> _logger;
    private readonly HttpClient _httpClient;
    private readonly KnowledgeSearchServiceSettings _settings;

    public KnowledgeService(
        IApplicationDbContext context,
        ILogger<KnowledgeService> logger,
        HttpClient httpClient,
        IOptions<KnowledgeSearchServiceSettings> settings)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClient;
        _settings = settings.Value;

        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogWarning("KnowledgeSearchServiceSettings.BaseUrl is not configured.");
        }
    }

    public async Task IndexFamilyData(Guid familyId)
    {
        var family = await _context.Families
            .FirstOrDefaultAsync(f => f.Id == familyId);

        if (family == null)
        {
            _logger.LogWarning("Family with ID {FamilyId} not found for indexing.", familyId);
            return;
        }

        var metadata = new Dictionary<string, object>
        {
            { "family_id", family.Id.ToString() },
            { "original_id", family.Id.ToString() },
            { "content_type", "Family" },
            { "name", family.Name },
            { "code", family.Code },
            { "description", family.Description ?? string.Empty },
            { "genealogy_record", family.GenealogyRecord ?? string.Empty },
            { "progenitor_name", family.ProgenitorName ?? string.Empty },
            { "family_covenant", family.FamilyCovenant ?? string.Empty },
            { "visibility", family.Visibility }
        };

        var summary = $"Gia đình họ {family.Name}, chi nhánh {(family.GenealogyRecord ?? "N/A")}.{Environment.NewLine}" +
                      $"Khu vực sinh sống chính: {(family.Address ?? "N/A")}.{Environment.NewLine}" +
                      $"Đặc điểm nổi bật: {(family.Description ?? "N/A")}.{Environment.NewLine}" +
                      $"Tổng số thành viên: {family.TotalMembers}.{Environment.NewLine}" +
                      $"Tổng số thế hệ: {family.TotalGenerations}.";

        var genericDto = new GenericKnowledgeDto { Metadata = metadata, Summary = summary };
        await SendToKnowledgeService(genericDto, "index");
    }

    public async Task IndexMemberData(Guid memberId)
    {
        var member = await _context.Members
            .Include(m => m.Family)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            _logger.LogWarning("Member with ID {MemberId} not found for indexing.", memberId);
            return;
        }

        // --- Fetch Parents ---
        var parentRelationships = await _context.Relationships
            .Where(r => r.TargetMemberId == member.Id && (r.Type == RelationshipType.Father || r.Type == RelationshipType.Mother))
            .ToListAsync();

        var parentIds = parentRelationships.Select(r => r.SourceMemberId).Distinct().ToList();
        var parents = await _context.Members
            .Where(m => parentIds.Contains(m.Id))
            .ToListAsync();

        var father = parents.FirstOrDefault(p => parentRelationships.Any(pr => pr.SourceMemberId == p.Id && pr.Type == RelationshipType.Father));
        var mother = parents.FirstOrDefault(p => parentRelationships.Any(pr => pr.SourceMemberId == p.Id && pr.Type == RelationshipType.Mother));

        var parentsSummary = "Con của N/A.";
        if (father != null && mother != null)
        {
            parentsSummary = $"Con của {father.FullName} và {mother.FullName}.";
        }
        else if (father != null)
        {
            parentsSummary = $"Con của {father.FullName}.";
        }
        else if (mother != null)
        {
            parentsSummary = $"Con của {mother.FullName}.";
        }


        // --- Fetch Spouse ---
        // A member can have multiple spouse relationships over time.
        // For simplicity, we'll pick the first one found or refine logic if needed for primary spouse.
        var spouseRelationship = await _context.Relationships
            .Where(r => (r.SourceMemberId == member.Id && (r.Type == RelationshipType.Husband || r.Type == RelationshipType.Wife)) ||
                        (r.TargetMemberId == member.Id && (r.Type == RelationshipType.Husband || r.Type == RelationshipType.Wife)))
            .FirstOrDefaultAsync();

        string spouseName = "N/A";
        string marriageYear = "N/A"; // Assuming Relationship doesn't have MarriageDate for now.
        if (spouseRelationship != null)
        {
            Guid spouseId = spouseRelationship.SourceMemberId == member.Id ? spouseRelationship.TargetMemberId : spouseRelationship.SourceMemberId;
            var spouse = await _context.Members.FirstOrDefaultAsync(m => m.Id == spouseId);
            if (spouse != null)
            {
                spouseName = spouse.FullName;
            }
            // If Relationship entity had a MarriageDate property, we would use it here:
            // marriageYear = spouseRelationship.MarriageDate?.Year.ToString() ?? "N/A";
        }

        var marriageSummary = $"Hôn nhân: kết hôn với {spouseName}{(marriageYear != "N/A" ? " năm " + marriageYear : "")}.";


        // --- Fetch Children ---
        var childRelationships = await _context.Relationships
            .Where(r => r.SourceMemberId == member.Id && (r.Type == RelationshipType.Father || r.Type == RelationshipType.Mother))
            .ToListAsync();

        var childIds = childRelationships.Select(r => r.TargetMemberId).Distinct().ToList();
        var children = await _context.Members
            .Where(m => childIds.Contains(m.Id))
            .OrderBy(m => m.DateOfBirth) // Optional: order children by birth date
            .ToListAsync();

        var childrenNames = children.Any() ? string.Join(", ", children.Select(c => c.FullName)) : "N/A";
        var childrenSummary = $"Con cái: {childrenNames}.";

        var metadata = new Dictionary<string, object>
        {
            { "family_id", member.FamilyId.ToString() },
            { "original_id", member.Id.ToString() },
            { "content_type", "Member" },
            { "member_id", member.Id.ToString() },
            { "full_name", member.FullName },
            { "code", member.Code },
            { "gender", member.Gender ?? string.Empty },
            { "biography", member.Biography ?? string.Empty },
            { "date_of_birth", member.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty },
            { "date_of_death", member.DateOfDeath?.ToString("yyyy-MM-dd") ?? string.Empty }
        };

        string genderVi = member.Gender switch
        {
            "Male" => "nam",
            "Female" => "nữ",
            _ => "N/A"
        };
        string status = member.IsDeceased ? "Đã mất" : "Còn sống";

        var summary = $"{member.FullName} ({genderVi}, sinh {member.DateOfBirth?.Year.ToString() ?? "N/A"}, mất {member.DateOfDeath?.Year.ToString() ?? "N/A"}).{Environment.NewLine}" +
                      $"Thuộc đời thứ {("N/A")} trong gia đình họ {member.Family?.Name ?? "N/A"}.{Environment.NewLine}" +
                      $"{parentsSummary}{Environment.NewLine}" +
                      $"{marriageSummary}{Environment.NewLine}" +
                      $"{childrenSummary}{Environment.NewLine}" +
                      $"Nơi sinh: {member.PlaceOfBirth ?? "N/A"}.{Environment.NewLine}" +
                      $"Nơi mất: {member.PlaceOfDeath ?? "N/A"}.{Environment.NewLine}" +
                      $"Nghề nghiệp: {member.Occupation ?? "N/A"}.{Environment.NewLine}" +
                      $"Tình trạng: {status}.{Environment.NewLine}" +
                      $"Tiểu sử: {member.Biography ?? "N/A"}.{Environment.NewLine}" +
                      $"Khu vực sinh sống chính: {member.Family?.Address ?? "N/A"}.{Environment.NewLine}" +
                      $"Là con thứ {(member.Order?.ToString() ?? "N/A")} trong nhà.";

        var genericDto = new GenericKnowledgeDto { Metadata = metadata, Summary = summary };
        await SendToKnowledgeService(genericDto, "index");
    }

    public async Task IndexEventData(Guid eventId)
    {
        var @event = await _context.Events
            .Include(e => e.Family)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (@event == null)
        {
            _logger.LogWarning("Event with ID {EventId} not found for indexing.", eventId);
            return;
        }

        var metadata = new Dictionary<string, object>
        {
            { "family_id", (@event.FamilyId ?? Guid.Empty).ToString() },
            { "original_id", @event.Id.ToString() },
            { "content_type", "Event" },
            { "event_id", @event.Id.ToString() },
            { "name", @event.Name },
            { "code", @event.Code },
            { "description", @event.Description ?? string.Empty },
            { "location", @event.Location ?? string.Empty },
            { "calendar_type", @event.CalendarType.ToString() },
            { "solar_date", @event.SolarDate?.ToString("yyyy-MM-dd") ?? string.Empty }
        };

        var summary = $"Sự kiện: {@event.Name}. Mã sự kiện: {@event.Code}.{Environment.NewLine}" +
                      $"Loại sự kiện: {@event.CalendarType.ToString() ?? "N/A"}.{Environment.NewLine}" +
                      $"Mô tả: {@event.Description ?? "N/A"}.{Environment.NewLine}" +
                      $"Địa điểm: {@event.Location ?? "N/A"}.{Environment.NewLine}" +
                      $"Ngày diễn ra: {@event.SolarDate?.ToShortDateString() ?? "N/A"}.{Environment.NewLine}" +
                      $"Thuộc gia đình họ: {@event.Family?.Name ?? "N/A"}.";

        var genericDto = new GenericKnowledgeDto { Metadata = metadata, Summary = summary };
        await SendToKnowledgeService(genericDto, "index");
    }

    public async Task DeleteFamilyData(Guid familyId)
    {
        var metadata = new Dictionary<string, object>
        {
            { "family_id", familyId.ToString() },
            { "original_id", familyId.ToString() },
            { "content_type", "Family" }
        };
        var genericDto = new GenericKnowledgeDto { Metadata = metadata, Summary = string.Empty }; // Summary not needed for delete
        await SendToKnowledgeService(genericDto, "delete");
    }

    public async Task DeleteMemberData(Guid memberId)
    {
        // For deletion, we need the family_id and the original_id (member_id)
        // If we don't have family_id from the member object, we might need to fetch it.
        // For simplicity here, assuming member_id is enough for the Python service's 'where_clause'
        var metadata = new Dictionary<string, object>
        {
            // { "family_id", "N/A" }, // If family_id is needed, fetch member first
            { "original_id", memberId.ToString() },
            { "content_type", "Member" }
        };
        var genericDto = new GenericKnowledgeDto { Metadata = metadata, Summary = string.Empty };
        await SendToKnowledgeService(genericDto, "delete");
    }

    public async Task DeleteEventData(Guid eventId)
    {
        var metadata = new Dictionary<string, object>
        {
            // { "family_id", "N/A" }, // If family_id is needed, fetch event first
            { "original_id", eventId.ToString() },
            { "content_type", "Event" }
        };
        var genericDto = new GenericKnowledgeDto { Metadata = metadata, Summary = string.Empty };
        await SendToKnowledgeService(genericDto, "delete");
    }


    private async Task SendToKnowledgeService(GenericKnowledgeDto data, string action)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogError("KnowledgeSearchService BaseUrl is not configured. Cannot send data to knowledge service.");
            return;
        }

        try
        {
            var request = new KnowledgeIndexRequest { Data = data, Action = action };
            var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var originalId = data.Metadata.TryGetValue("original_id", out var id) ? id.ToString() : "N/A";
            var contentType = data.Metadata.TryGetValue("content_type", out var type) ? type.ToString() : "N/A";

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v1/knowledge/index", httpContent);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully sent {Action} {ContentType} data to knowledge service. Original ID: {OriginalId}", action, contentType, originalId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error sending {Action} {ContentType} data to knowledge service. Original ID: {OriginalId}. Status Code: {StatusCode}", action, data.Metadata.TryGetValue("content_type", out var type) ? type.ToString() : "N/A", data.Metadata.TryGetValue("original_id", out var id) ? id.ToString() : "N/A", ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while sending {Action} {ContentType} data to knowledge service. Original ID: {OriginalId}.", action, data.Metadata.TryGetValue("content_type", out var type) ? type.ToString() : "N/A", data.Metadata.TryGetValue("original_id", out var id) ? id.ToString() : "N/A");
        }
    }

    public async Task<List<KnowledgeSearchResultDto>> SearchKnowledgeBase(Guid familyId, string queryString, int topK, List<string> allowedVisibility)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogError("KnowledgeSearchService BaseUrl is not configured. Cannot perform search.");
            return new List<KnowledgeSearchResultDto>();
        }

        try
        {
            var requestBody = new
            {
                family_id = familyId.ToString(),
                query = queryString,
                top_k = topK,
                allowed_visibility = allowedVisibility
            };

            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
            var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v1/search", httpContent);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<KnowledgeSearchApiResponse>(responseContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

            _logger.LogInformation("Successfully performed search in knowledge service for FamilyId: {FamilyId}, Query: {QueryString}", familyId, queryString);

            return searchResponse?.Results ?? new List<KnowledgeSearchResultDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error performing search in knowledge service for FamilyId: {FamilyId}, Query: {QueryString}", familyId, queryString);
            return new List<KnowledgeSearchResultDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while performing search in knowledge service for FamilyId: {FamilyId}, Query: {QueryString}", familyId, queryString);
            return new List<KnowledgeSearchResultDto>();
        }
    }

    // Helper class to deserialize the API response from knowledge-search-service
    private class KnowledgeSearchApiResponse
    {
        public List<KnowledgeSearchResultDto>? Results { get; set; }
    }
}
