using backend.Application.Common.Interfaces;
using backend.Application.Knowledge.DTOs;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using backend.Application.Common.Models.AppSetting;
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

        var dto = new FamilyKnowledgeDto
        {
            FamilyId = family.Id,
            Name = family.Name,
            Code = family.Code,
            Description = family.Description,
            GenealogyRecord = family.GenealogyRecord,
            ProgenitorName = family.ProgenitorName,
            FamilyCovenant = family.FamilyCovenant,
            Visibility = family.Visibility
        };

        await SendToKnowledgeService(dto, "family-data", "index");
    }

    public async Task IndexMemberData(Guid memberId)
    {
        var member = await _context.Members
            .Include(m => m.Family) // To get FamilyId
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            _logger.LogWarning("Member with ID {MemberId} not found for indexing.", memberId);
            return;
        }

        var dto = new MemberKnowledgeDto
        {
            FamilyId = member.FamilyId,
            MemberId = member.Id,
            FullName = member.FullName,
            Code = member.Code,
            Gender = member.Gender,
            Biography = member.Biography,
            DateOfBirth = member.DateOfBirth,
            DateOfDeath = member.DateOfDeath
        };

        await SendToKnowledgeService(dto, "member-data", "index");
    }

    public async Task IndexEventData(Guid eventId)
    {
        var @event = await _context.Events
            .Include(e => e.Family) // To get FamilyId
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (@event == null)
        {
            _logger.LogWarning("Event with ID {EventId} not found for indexing.", eventId);
            return;
        }

        var dto = new EventKnowledgeDto
        {
            FamilyId = @event.FamilyId ?? Guid.Empty, // Assuming events can be without a family or handle appropriately
            EventId = @event.Id,
            Name = @event.Name,
            Code = @event.Code,
            Description = @event.Description,
            Location = @event.Location,
            CalendarType = @event.CalendarType.ToString(),
            SolarDate = @event.SolarDate
        };

        await SendToKnowledgeService(dto, "event-data", "index");
    }

    public async Task DeleteFamilyData(Guid familyId)
    {
        var dto = new FamilyKnowledgeDto { FamilyId = familyId }; // Only need ID for deletion
        await SendToKnowledgeService(dto, "family-data", "delete");
    }

    public async Task DeleteMemberData(Guid memberId)
    {
        var dto = new MemberKnowledgeDto { MemberId = memberId }; // Only need ID for deletion
        await SendToKnowledgeService(dto, "member-data", "delete");
    }

    public async Task DeleteEventData(Guid eventId)
    {
        var dto = new EventKnowledgeDto { EventId = eventId }; // Only need ID for deletion
        await SendToKnowledgeService(dto, "event-data", "delete");
    }


    private async Task SendToKnowledgeService<T>(T data, string endpointSuffix, string action) where T : class
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogError("KnowledgeSearchService BaseUrl is not configured. Cannot send data to knowledge service.");
            return;
        }

        try
        {
            var request = new KnowledgeIndexRequest<T> { Data = data, Action = action };
            var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/{endpointSuffix}", httpContent);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully sent {Action} {DataType} data to knowledge service. ID: {Id}", action, typeof(T).Name, GetIdFromData(data));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error sending {Action} {DataType} data to knowledge service. Status Code: {StatusCode}", action, typeof(T).Name, ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while sending {Action} {DataType} data to knowledge service.", action, typeof(T).Name);
        }
    }

    private object? GetIdFromData<T>(T data) where T : class
    {
        if (data is FamilyKnowledgeDto family) return family.FamilyId;
        if (data is MemberKnowledgeDto member) return member.MemberId;
        if (data is EventKnowledgeDto @event) return @event.EventId;
        return null;
    }
}
