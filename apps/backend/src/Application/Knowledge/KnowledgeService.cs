using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Knowledge.DTOs;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

        var summary = $"Gia đình họ {family.Name}, chi nhánh {family.GenealogyRecord ?? "N/A"}.{Environment.NewLine}" +
                      $"Khu vực sinh sống chính: {family.Address ?? "N/A"}.{Environment.NewLine}" +
                      $"Đặc điểm nổi bật: {family.Description ?? "N/A"}.{Environment.NewLine}" +
                      $"Tổng số thế hệ: {family.TotalGenerations}.";
        var vectorData = new VectorData
        {
            FamilyId = family.Id.ToString(),
            EntityId = family.Id.ToString(),
            Type = "family",
            Visibility = family.Visibility,
            Name = family.Name,
            Summary = summary,
            Metadata = metadata
        };
        var requestBody = new KnowledgeAddRequest { Data = vectorData };

        try
        {
            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
            var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v1/knowledge", httpContent);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully indexed family data for FamilyId: {FamilyId}", family.Id);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error indexing family data for FamilyId: {FamilyId}. Status Code: {StatusCode}", family.Id, ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while indexing family data for FamilyId: {FamilyId}.", family.Id);
        }
    }

    public async Task UpsertFamilyData(Guid familyId)
    {
        var family = await _context.Families
            .FirstOrDefaultAsync(f => f.Id == familyId);

        if (family == null)
        {
            _logger.LogWarning("Family with ID {FamilyId} not found for upserting.", familyId);
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

        var summary = $"Gia đình họ {family.Name}, chi nhánh {family.GenealogyRecord ?? "N/A"}.{Environment.NewLine}" +
                      $"Khu vực sinh sống chính: {family.Address ?? "N/A"}.{Environment.NewLine}" +
                      $"Đặc điểm nổi bật: {family.Description ?? "N/A"}.{Environment.NewLine}" +
                      $"Tổng số thế hệ: {family.TotalGenerations}.";
        var vectorData = new VectorData
        {
            FamilyId = family.Id.ToString(),
            EntityId = family.Id.ToString(),
            Type = "family",
            Visibility = family.Visibility,
            Name = family.Name,
            Summary = summary,
            Metadata = metadata
        };
        var requestBody = new KnowledgeAddRequest { Data = vectorData };

        try
        {
            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
            var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v1/knowledge/upsert", httpContent);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully upserted family data for FamilyId: {FamilyId}", family.Id);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error upserting family data for FamilyId: {FamilyId}. Status Code: {StatusCode}", family.Id, ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while upserting family data for FamilyId: {FamilyId}.", family.Id);
        }
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
                      $"Khu vực sinh sống chính: {member.Family?.Address ?? "N/A"}.{Environment.NewLine}";
        var vectorData = new VectorData
        {
            FamilyId = member.FamilyId.ToString(),
            EntityId = member.Id.ToString(),
            Type = "member",
            Visibility = "public", // Assuming public for now, adjust if member has visibility
            Name = member.FullName,
            Summary = summary,
            Metadata = metadata
        };
        var requestBody = new KnowledgeAddRequest { Data = vectorData };

        try
        {
            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
            var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v1/knowledge", httpContent);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully indexed member data for MemberId: {MemberId}", member.Id);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error indexing member data for MemberId: {MemberId}. Status Code: {StatusCode}", member.Id, ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while indexing member data for MemberId: {MemberId}.", member.Id);
        }
    }

    public async Task UpsertMemberData(Guid memberId)
    {
        var member = await _context.Members
            .Include(m => m.Family)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            _logger.LogWarning("Member with ID {MemberId} not found for upserting.", memberId);
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
                      $"Khu vực sinh sống chính: {member.Family?.Address ?? "N/A"}.{Environment.NewLine}";
        var vectorData = new VectorData
        {
            FamilyId = member.FamilyId.ToString(),
            EntityId = member.Id.ToString(),
            Type = "member",
            Visibility = "public", // Assuming public for now, adjust if member has visibility
            Name = member.FullName,
            Summary = summary,
            Metadata = metadata
        };
        var requestBody = new KnowledgeAddRequest { Data = vectorData };

        try
        {
            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
            var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v1/knowledge/upsert", httpContent);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully upserted member data for MemberId: {MemberId}", member.Id);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error upserting member data for MemberId: {MemberId}. Status Code: {StatusCode}", member.Id, ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while upserting member data for MemberId: {MemberId}.", member.Id);
        }
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

        var vectorData = new VectorData
        {
            FamilyId = (@event.FamilyId ?? Guid.Empty).ToString(),
            EntityId = @event.Id.ToString(),
            Type = "event",
            Visibility = "public", // Assuming public for now, adjust if event has visibility
            Name = @event.Name,
            Summary = summary,
            Metadata = metadata
        };
        var requestBody = new KnowledgeAddRequest { Data = vectorData };

        try
        {
            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
            var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v1/knowledge", httpContent);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully indexed event data for EventId: {EventId}", @event.Id);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error indexing event data for EventId: {EventId}. Status Code: {StatusCode}", @event.Id, ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while indexing event data for EventId: {EventId}.", @event.Id);
        }
    }

    public async Task UpsertEventData(Guid eventId)
    {
        var @event = await _context.Events
            .Include(e => e.Family)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (@event == null)
        {
            _logger.LogWarning("Event with ID {EventId} not found for upserting.", eventId);
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

        var vectorData = new VectorData
        {
            FamilyId = (@event.FamilyId ?? Guid.Empty).ToString(),
            EntityId = @event.Id.ToString(),
            Type = "event",
            Visibility = "public", // Assuming public for now, adjust if event has visibility
            Name = @event.Name,
            Summary = summary,
            Metadata = metadata
        };
        var requestBody = new KnowledgeAddRequest { Data = vectorData };

        try
        {
            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
            var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v1/knowledge/upsert", httpContent);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully upserted event data for EventId: {EventId}", @event.Id);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error upserting event data for EventId: {EventId}. Status Code: {StatusCode}", @event.Id, ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while upserting event data for EventId: {EventId}.", @event.Id);
        }
    }

    public async Task DeleteFamilyData(Guid familyId)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogError("KnowledgeSearchService BaseUrl is not configured. Cannot delete family data.");
            return;
        }

        try
        {
            var response = await _httpClient.DeleteAsync($"{_settings.BaseUrl}/api/v1/knowledge/{familyId}/{familyId}");
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully deleted family data for FamilyId: {FamilyId}", familyId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error deleting family data for FamilyId: {FamilyId}. Status Code: {StatusCode}", familyId, ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while deleting family data for FamilyId: {FamilyId}.", familyId);
        }
    }

    public async Task DeleteMemberData(Guid memberId)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogError("KnowledgeSearchService BaseUrl is not configured. Cannot delete member data.");
            return;
        }
        var member = await _context.Members.FindAsync(memberId);
        if (member == null)
        {
            _logger.LogWarning("Member with ID {MemberId} not found for deletion from knowledge service.", memberId);
            return;
        }

        try
        {
            var response = await _httpClient.DeleteAsync($"{_settings.BaseUrl}/api/v1/knowledge/{member.FamilyId}/{memberId}");
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully deleted member data for MemberId: {MemberId} in FamilyId: {FamilyId}", memberId, member.FamilyId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error deleting member data for MemberId: {MemberId} in FamilyId: {FamilyId}. Status Code: {StatusCode}", memberId, member.FamilyId, ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while deleting member data for MemberId: {MemberId} in FamilyId: {FamilyId}.", memberId, member.FamilyId);
        }
    }

    public async Task DeleteEventData(Guid eventId)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogError("KnowledgeSearchService BaseUrl is not configured. Cannot delete event data.");
            return;
        }
        var @event = await _context.Events.FindAsync(eventId);
        if (@event == null || !@event.FamilyId.HasValue)
        {
            _logger.LogWarning("Event with ID {EventId} not found or has no associated FamilyId for deletion from knowledge service.", eventId);
            return;
        }

        try
        {
            var response = await _httpClient.DeleteAsync($"{_settings.BaseUrl}/api/v1/knowledge/{@event.FamilyId.Value}/{eventId}");
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully deleted event data for EventId: {EventId} in FamilyId: {FamilyId}", eventId, @event.FamilyId.Value);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error deleting event data for EventId: {EventId} in FamilyId: {FamilyId}. Status Code: {StatusCode}", eventId, @event.FamilyId.Value, ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while deleting event data for EventId: {EventId} in FamilyId: {FamilyId}.", eventId, @event.FamilyId.Value);
        }
    }

    public async Task DeleteKnowledgeByFamilyId(Guid familyId)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogError("KnowledgeSearchService BaseUrl is not configured. Cannot delete knowledge by family ID.");
            return;
        }

        try
        {
            var response = await _httpClient.DeleteAsync($"{_settings.BaseUrl}/api/v1/knowledge/family-data/{familyId}");
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully deleted all knowledge data for FamilyId: {FamilyId}", familyId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error deleting all knowledge data for FamilyId: {FamilyId}. Status Code: {StatusCode}", familyId, ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while deleting all knowledge data for FamilyId: {FamilyId}.", familyId);
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



    public async Task<string> IndexMemberFaceData(MemberFaceDto faceData)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogError("KnowledgeSearchService BaseUrl is not configured. Cannot index face data.");
            return string.Empty;
        }

        try
        {
            var requestBody = new FaceAddRequest
            {
                FamilyId = faceData.FamilyId,
                MemberId = faceData.MemberId,
                FaceId = faceData.FaceId,
                BoundingBox = faceData.BoundingBox,
                Confidence = faceData.Confidence,
                ThumbnailUrl = faceData.ThumbnailUrl,
                OriginalImageUrl = faceData.OriginalImageUrl,
                Embedding = faceData.Embedding,
                Emotion = faceData.Emotion,
                EmotionConfidence = faceData.EmotionConfidence,
                IsVectorDbSynced = faceData.IsVectorDbSynced,
                VectorDbId = faceData.VectorDbId
            };

            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
            var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v1/faces", httpContent);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var faceAddResponse = JsonSerializer.Deserialize<FaceAddApiResponse>(responseContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

            if (faceAddResponse?.VectorDbId == null)
            {
                _logger.LogError("KnowledgeSearchService did not return VectorDbId for FaceId: {FaceId}", faceData.FaceId);
                return string.Empty;
            }

            _logger.LogInformation("Successfully indexed face data for FaceId: {FaceId}. Returned VectorDbId: {VectorDbId}", faceData.FaceId, faceAddResponse.VectorDbId);
            return faceAddResponse.VectorDbId;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error indexing face data for FaceId: {FaceId}. Status Code: {StatusCode}", faceData.FaceId, ex.StatusCode);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while indexing face data for FaceId: {FaceId}.", faceData.FaceId);
            return string.Empty;
        }
    }

    public async Task DeleteMemberFaceData(Guid familyId, Guid faceId)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogError("KnowledgeSearchService BaseUrl is not configured. Cannot delete face data.");
            return;
        }

        try
        {
            var response = await _httpClient.DeleteAsync($"{_settings.BaseUrl}/api/v1/faces/{familyId}/{faceId}");
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully deleted face data for FaceId: {FaceId} in FamilyId: {FamilyId}", faceId, familyId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error deleting face data for FaceId: {FaceId} in FamilyId: {FamilyId}. Status Code: {StatusCode}", faceId, familyId, ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while deleting face data for FaceId: {FaceId} in FamilyId: {FamilyId}.", faceId, familyId);
        }
    }

    public async Task DeleteMemberFacesByMemberId(Guid familyId, Guid memberId)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogError("KnowledgeSearchService BaseUrl is not configured. Cannot delete member faces.");
            return;
        }

        try
        {
            var response = await _httpClient.DeleteAsync($"{_settings.BaseUrl}/api/v1/faces/member/{familyId}/{memberId}");
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully deleted all faces for MemberId: {MemberId} in FamilyId: {FamilyId}", memberId, familyId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error deleting all faces for MemberId: {MemberId} in FamilyId: {FamilyId}. Status Code: {StatusCode}", memberId, familyId, ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while deleting all faces for MemberId: {MemberId} in FamilyId: {FamilyId}.", memberId, familyId);
        }
    }

    public async Task DeleteFamilyFacesData(Guid familyId)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogError("KnowledgeSearchService BaseUrl is not configured. Cannot delete family faces.");
            return;
        }

        try
        {
            var response = await _httpClient.DeleteAsync($"{_settings.BaseUrl}/api/v1/faces/family/{familyId}");
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully deleted all faces for FamilyId: {FamilyId}", familyId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error deleting all faces for FamilyId: {FamilyId}. Status Code: {StatusCode}", familyId, ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while deleting all faces for FamilyId: {FamilyId}.", familyId);
        }
    }

    public async Task<List<FaceSearchResultDto>> SearchFaces(Guid familyId, List<double> embedding, Guid? memberId = null, int topK = 5)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogError("KnowledgeSearchService BaseUrl is not configured. Cannot perform face search.");
            return new List<FaceSearchResultDto>();
        }

        try
        {
            var requestBody = new FaceSearchRequest
            {
                FamilyId = familyId,
                QueryEmbedding = embedding,
                MemberId = memberId,
                TopK = topK
            };

            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
            var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v1/faces/search", httpContent);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<FaceSearchApiResponse>(responseContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
            _logger.LogInformation("Successfully performed face search in knowledge service for FamilyId: {FamilyId}. Found {ResultCount} faces.", familyId, searchResponse?.Results?.Count ?? 0);
            return searchResponse?.Results ?? new List<FaceSearchResultDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error performing face search in knowledge service for FamilyId: {FamilyId}. Status Code: {StatusCode}", familyId, ex.StatusCode);
            return new List<FaceSearchResultDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while performing face search in knowledge service for FamilyId: {FamilyId}.", familyId);
            return new List<FaceSearchResultDto>();
        }
    }
}

