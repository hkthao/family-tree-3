using System.Text.Json;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Knowledge;
using backend.Application.Knowledge.DTOs;
using backend.Application.UnitTests.Common; // Assuming TestBase is here
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.ValueObjects; // Add this
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected; // For Protected methods
using Xunit;

namespace backend.Application.UnitTests.Knowledge;

public class KnowledgeServiceTests : TestBase // Assuming TestBase provides basic setup
{
    private readonly Mock<ILogger<KnowledgeService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<IOptions<KnowledgeSearchServiceSettings>> _mockSettings;
    private readonly KnowledgeService _knowledgeService;
    public KnowledgeServiceTests()
    {
        _mockLogger = new Mock<ILogger<KnowledgeService>>();

        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:8000/") // Base URL for the mock HttpClient
        };

        _mockSettings = new Mock<IOptions<KnowledgeSearchServiceSettings>>();
        _mockSettings.Setup(s => s.Value).Returns(new KnowledgeSearchServiceSettings { BaseUrl = "http://localhost:8000" });

        _knowledgeService = new KnowledgeService(
            _context, // Use the _context from TestBase
            _mockLogger.Object,
            _httpClient,
            _mockSettings.Object
        );
    }


    // --- Tests for UpsertFamilyData ---
    [Fact]
    public async Task UpsertFamilyData_ShouldSendCorrectGenericDtoToUpsertEndpoint()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = Family.Create("Nguyễn", "NguyenFamilyCode", "Mô tả", "Hà Nội", "Public", Guid.NewGuid());
        family.Id = familyId;
        family.GenealogyRecord = "Chi nhánh Hà Nội";
        family.FamilyCovenant = "Đoàn kết";
        family.TotalMembers = 10;
        family.TotalGenerations = 5;
        family.Address = "Hà Nội"; // Ensure Address is set for summary verification

        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri == new Uri("http://localhost:8000/api/v1/knowledge/upsert")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"message\": \"success\"}")
            });

        // Act
        await _knowledgeService.UpsertFamilyData(familyId);

        // Assert
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri == new Uri("http://localhost:8000/api/v1/knowledge/upsert") &&
                req.Content != null &&
                VerifyFamilyUpsertRequestContent(req.Content, family)
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    // --- Helper for verifying Family Upsert Request content ---
    private bool VerifyFamilyUpsertRequestContent(HttpContent content, Family family)
    {
        if (content == null) return false;
        var json = content.ReadAsStringAsync().Result;
        var request = JsonSerializer.Deserialize<KnowledgeAddRequest>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        request.Should().NotBeNull();
        request?.Data.Should().NotBeNull();
        request?.Data.Metadata.Should().NotBeNull();
        var metadata = (Dictionary<string, object>)request?.Data.Metadata!;
        metadata["family_id"].ToString().Should().Be(family.Id.ToString());
        metadata["original_id"].ToString().Should().Be(family.Id.ToString());
        metadata["content_type"].ToString().Should().Be("Family");
        metadata["name"].ToString().Should().Be(family.Name);
        metadata["code"].ToString().Should().Be(family.Code);
        metadata["description"].ToString().Should().Be(family.Description);
        metadata["genealogy_record"].ToString().Should().Be(family.GenealogyRecord);
        metadata["progenitor_name"].ToString().Should().Be(family.ProgenitorName ?? string.Empty);
        metadata["family_covenant"].ToString().Should().Be(family.FamilyCovenant);
        metadata["visibility"].ToString().Should().Be(family.Visibility);

        request?.Data.FamilyId.Should().Be(family.Id.ToString());
        request?.Data.EntityId.Should().Be(family.Id.ToString());
        request?.Data.Type.Should().Be("family");
        request?.Data.Visibility.Should().Be(family.Visibility);
        request?.Data.Name.Should().Be(family.Name);

        var expectedSummary = $"Gia đình họ {family.Name}, chi nhánh {family.GenealogyRecord ?? "N/A"}.{Environment.NewLine}" +
                              $"Khu vực sinh sống chính: {family.Address ?? "N/A"}.{Environment.NewLine}" +
                              $"Đặc điểm nổi bật: {family.Description ?? "N/A"}.{Environment.NewLine}" +
                              $"Tổng số thế hệ: {family.TotalGenerations}.";
        request?.Data.Summary.Should().Be(expectedSummary);

        return true;
    }


    // --- Tests for UpsertMemberData ---
    [Fact]
    public async Task UpsertMemberData_ShouldSendCorrectGenericDtoToUpsertEndpoint()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var family = Family.Create("Nguyễn", "NguyenFamilyCode", "Mô tả", "Hà Nội", "Public", Guid.NewGuid());
        var member = new Member(memberId, "Văn A", "Nguyễn", "M001", familyId, family, false);
        member.Update("Văn A", "Nguyễn", "M001", "A", "Male", new DateTime(1955, 1, 1), new DateTime(2018, 1, 1), new LunarDate(1, 1, false, false), "Nghệ An", "TPHCM", null, null, "Hồ Chí Minh", "Kỹ sư", null, "Tiểu sử ông A", 1, true);

        var fatherId = Guid.NewGuid();
        var motherId = Guid.NewGuid();
        var spouseId = Guid.NewGuid();
        var child1Id = Guid.NewGuid();
        var child2Id = Guid.NewGuid();

        var father = new Member(fatherId, "Văn B", "Nguyễn", "M002", familyId, family, false); father.Update("Văn B", "Nguyễn", "M002", null, "Male", null, null, null, null, null, null, null, null, null, null, null, null, false);
        var mother = new Member(motherId, "Thị C", "Trần", "M003", familyId, family, false); mother.Update("Thị C", "Trần", "M003", null, "Female", null, null, null, null, null, null, null, null, null, null, null, null, false);
        var spouse = new Member(spouseId, "Thị D", "Lê", "M004", familyId, family, false); spouse.Update("Thị D", "Lê", "M004", null, "Female", null, null, null, null, null, null, null, null, null, null, null, null, false);
        var child1 = new Member(child1Id, "Văn E", "Nguyễn", "M005", familyId, family, false); child1.Update("Văn E", "Nguyễn", "M005", null, "Male", null, null, null, null, null, null, null, null, null, null, null, null, false);
        var child2 = new Member(child2Id, "Thị F", "Nguyễn", "M006", familyId, family, false); child2.Update("Thị F", "Nguyễn", "M006", null, "Female", null, null, null, null, null, null, null, null, null, null, null, null, false);


        var relationships = new List<Relationship>

                {

                    new Relationship(familyId, fatherId, memberId, RelationshipType.Father),

                    new Relationship(familyId, motherId, memberId, RelationshipType.Mother),

                    new Relationship(familyId, memberId, spouseId, RelationshipType.Husband), // Assuming member is husband

                    new Relationship(familyId, memberId, child1Id, RelationshipType.Father),

                    new Relationship(familyId, memberId, child2Id, RelationshipType.Father)

                };



        var members = new List<Member> { member, father, mother, spouse, child1, child2 }; // Inserted here



        _context.Families.Add(family); // Add the family as well
        _context.Members.AddRange(members);
        _context.Relationships.AddRange(relationships);
        await _context.SaveChangesAsync();


        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri == new Uri("http://localhost:8000/api/v1/knowledge/upsert")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"message\": \"success\"}")
            });

        // Act
        await _knowledgeService.UpsertMemberData(memberId);

        // Assert
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri == new Uri("http://localhost:8000/api/v1/knowledge/upsert") &&
                req.Content != null &&
                VerifyMemberUpsertRequestContent(req.Content, member, father, mother, spouse, new List<Member> { child1, child2 })
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    // --- Helper for verifying Member Upsert Request content ---
    private bool VerifyMemberUpsertRequestContent(HttpContent content, Member member, Member father, Member mother, Member spouse, List<Member> children)
    {
        if (content == null) return false;
        var json = content.ReadAsStringAsync().Result;
        var request = JsonSerializer.Deserialize<KnowledgeAddRequest>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        request.Should().NotBeNull();
        request?.Data.Should().NotBeNull();
        request?.Data.Metadata.Should().NotBeNull();
        var metadata = (Dictionary<string, object>)request?.Data.Metadata!;
        metadata["family_id"].ToString().Should().Be(member.FamilyId.ToString());
        metadata["original_id"].ToString().Should().Be(member.Id.ToString());
        metadata["content_type"].ToString().Should().Be("Member");
        metadata["full_name"].ToString().Should().Be(member.FullName);
        metadata["code"].ToString().Should().Be(member.Code);
        metadata["gender"].ToString().Should().Be(member.Gender);
        metadata["biography"].ToString().Should().Be(member.Biography);
        metadata["date_of_birth"].ToString().Should().Be(member.DateOfBirth?.ToString("yyyy-MM-dd"));
        metadata["date_of_death"].ToString().Should().Be(member.DateOfDeath?.ToString("yyyy-MM-dd") ?? string.Empty);
        metadata["lunar_date_of_death_day"].ToString().Should().Be(member.LunarDateOfDeath?.Day.ToString() ?? string.Empty);
        metadata["lunar_date_of_death_month"].ToString().Should().Be(member.LunarDateOfDeath?.Month.ToString() ?? string.Empty);
        string expectedIsLeapMonthString = member.LunarDateOfDeath?.IsLeapMonth?.ToString() ?? string.Empty;

        string actualIsLeapMonthString = (metadata.TryGetValue("lunar_date_of_death_leap_month", out object? leapMonthObj) && leapMonthObj != null)
            ? leapMonthObj.ToString() ?? string.Empty
            : string.Empty;

        actualIsLeapMonthString.Should().Be(expectedIsLeapMonthString);


        request?.Data.FamilyId.Should().Be(member.FamilyId.ToString());
        request?.Data.EntityId.Should().Be(member.Id.ToString());
        request?.Data.Type.Should().Be("member");
        request?.Data.Visibility.Should().Be("public");
        request?.Data.Name.Should().Be(member.FullName);

        string genderVi = member.Gender switch
        {
            "Male" => "nam",
            "Female" => "nữ",
            _ => "N/A"
        };
        var parentsSummary = $"Con của {father.FullName} và {mother.FullName}.";
        var marriageSummary = $"Hôn nhân: kết hôn với {spouse.FullName}.";
        var childrenNames = string.Join(", ", children.Select(c => c.FullName));
        var childrenSummary = $"Con cái: {childrenNames}.";
        var isLeapMonth = member.LunarDateOfDeath != null && member.LunarDateOfDeath.IsLeapMonth.GetValueOrDefault() ? " nhuận" : "";

        var expectedSummary = $"{member.FullName} ({genderVi}, sinh {member.DateOfBirth?.Year.ToString() ?? "N/A"}, mất {member.DateOfDeath?.Year.ToString() ?? "N/A"}" +
                              $"{(member.LunarDateOfDeath != null ? $" (âm lịch ngày {member.LunarDateOfDeath.Day} tháng {member.LunarDateOfDeath.Month}{isLeapMonth})" : string.Empty)}).{Environment.NewLine}" +
                              $"Thuộc đời thứ {"N/A"} trong gia đình họ {member.Family?.Name ?? "N/A"}.{Environment.NewLine}" +
                              $"{parentsSummary}{Environment.NewLine}" +
                              $"{marriageSummary}{Environment.NewLine}" +
                              $"{childrenSummary}{Environment.NewLine}" +
                              $"Nơi sinh: {member.PlaceOfBirth ?? "N/A"}.{Environment.NewLine}" +
                              $"Nơi mất: {member.PlaceOfDeath ?? "N/A"}.{Environment.NewLine}" +
                              $"Nghề nghiệp: {member.Occupation ?? "N/A"}.{Environment.NewLine}" +
                              $"Tình trạng: {(member.IsDeceased ? "Đã mất" : "Còn sống")}.{Environment.NewLine}" +
                              $"Tiểu sử: {member.Biography ?? "N/A"}.{Environment.NewLine}" +
                              $"Khu vực sinh sống chính: {member.Family?.Address ?? "N/A"}.{Environment.NewLine}";

        request?.Data.Summary.Should().Be(expectedSummary);

        return true;
    }

    // --- Tests for UpsertEventData ---
    [Fact]
    public async Task UpsertEventData_ShouldSendCorrectGenericDtoToUpsertEndpoint()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = Family.Create("Nguyễn", "NguyenFamilyCode", "Mô tả", "Hà Nội", "Public", Guid.NewGuid());
        family.Id = familyId;

        var @event = Event.CreateSolarEvent(
            "Lễ giỗ tổ",
            "LG001",
            EventType.Anniversary,
            new DateTime(2023, 10, 20),
            RepeatRule.None, // Assuming RepeatRule.None as a default for testing
            familyId,
            "Mô tả lễ giỗ",
            "#FF0000", // Example color
            "Hà Nội"
        );
        var eventId = @event.Id; // Ensure eventId matches the created event's Id


        _context.Families.Add(family); // Add the family as well
        _context.Events.Add(@event); // Add the event
        await _context.SaveChangesAsync();

        _context.Events.Any(e => e.Id == eventId).Should().BeTrue(); // Verify event is queryable from context
        @event.Should().NotBeNull(); // Verify event is in context

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri == new Uri("http://localhost:8000/api/v1/knowledge/upsert")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"message\": \"success\"}")
            });

        // Act
        await _knowledgeService.UpsertEventData(eventId);

        // Assert
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri == new Uri("http://localhost:8000/api/v1/knowledge/upsert") &&
                req.Content != null &&
                VerifyEventUpsertRequestContent(req.Content, @event)
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    // --- Helper for verifying Event Upsert Request content ---
    private bool VerifyEventUpsertRequestContent(HttpContent content, Event @event)
    {
        if (content == null) return false;
        var json = content.ReadAsStringAsync().Result;
        var request = JsonSerializer.Deserialize<KnowledgeAddRequest>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        request.Should().NotBeNull();
        request?.Data.Should().NotBeNull();
        var metadata = request?.Data.Metadata!;
        metadata["family_id"].ToString().Should().Be((@event.FamilyId ?? Guid.Empty).ToString());
        metadata["original_id"].ToString().Should().Be(@event.Id.ToString());
        metadata["content_type"].ToString().Should().Be("Event");
        metadata["event_id"].ToString().Should().Be(@event.Id.ToString());
        metadata["name"].ToString().Should().Be(@event.Name);
        metadata["code"].ToString().Should().Be(@event.Code);
        metadata["description"].ToString().Should().Be(@event.Description);
        metadata["location"].ToString().Should().Be(@event.Location);
        metadata["calendar_type"].ToString().Should().Be(@event.CalendarType.ToString());
        metadata["solar_date"].ToString().Should().Be(@event.SolarDate?.ToString("yyyy-MM-dd"));

        request?.Data.FamilyId.Should().Be((@event.FamilyId ?? Guid.Empty).ToString());
        request?.Data.EntityId.Should().Be(@event.Id.ToString());
        request?.Data.Type.Should().Be("event");
        request?.Data.Visibility.Should().Be("public");
        request?.Data.Name.Should().Be(@event.Name);

        var expectedSummary = $"Sự kiện: {@event.Name}. Mã sự kiện: {@event.Code}.{Environment.NewLine}" +
                              $"Loại sự kiện: {@event.CalendarType.ToString() ?? "N/A"}.{Environment.NewLine}" +
                              $"Mô tả: {@event.Description ?? "N/A"}.{Environment.NewLine}" +
                              $"Địa điểm: {@event.Location ?? "N/A"}.{Environment.NewLine}" +
                              $"Ngày diễn ra: {@event.SolarDate?.ToShortDateString() ?? "N/A"}.{Environment.NewLine}" +
                              $"Thuộc gia đình họ: {@event.Family?.Name ?? "N/A"}.";

        request?.Data.Summary.Should().Be(expectedSummary);

        return true;
    }

    // --- Tests for DeleteFamilyData ---
    [Fact]
    public async Task DeleteFamilyData_ShouldSendDeleteRequestToCorrectEndpoint()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri == new Uri($"http://localhost:8000/api/v1/knowledge/{familyId}/{familyId}")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            });

        // Act
        await _knowledgeService.DeleteFamilyData(familyId);

        // Assert
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Delete &&
                req.RequestUri == new Uri($"http://localhost:8000/api/v1/knowledge/{familyId}/{familyId}")
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    // --- Tests for DeleteMemberData ---
    [Fact]
    public async Task DeleteMemberData_ShouldSendDeleteRequestToCorrectEndpoint()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid(); // Members need a familyId
        var family = Family.Create("Nguyễn", "NguyenFamilyCode", "Mô tả", "Hà Nội", "Public", Guid.NewGuid());
        family.Id = familyId;
        var member = new Member
        {
            Id = memberId,
            LastName = "Văn A",
            FirstName = "Nguyễn",
            Code = "M001",
            FamilyId = familyId,
            Family = family,
            IsDeceased = false // Explicitly set for clarity
        };

        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();


        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri == new Uri($"http://localhost:8000/api/v1/knowledge/{familyId}/{memberId}")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            });

        // Act
        await _knowledgeService.DeleteMemberData(memberId);

        // Assert
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Delete &&
                req.RequestUri == new Uri($"http://localhost:8000/api/v1/knowledge/{familyId}/{memberId}")
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }



    // --- Tests for DeleteKnowledgeByFamilyId ---
    [Fact]
    public async Task DeleteKnowledgeByFamilyId_ShouldSendDeleteRequestToCorrectEndpoint()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri == new Uri($"http://localhost:8000/api/v1/knowledge/family-data/{familyId}")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            });

        // Act
        await _knowledgeService.DeleteKnowledgeByFamilyId(familyId);

        // Assert
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Delete &&
                req.RequestUri == new Uri($"http://localhost:8000/api/v1/knowledge/family-data/{familyId}")
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }


    // --- Tests for SearchKnowledgeBase ---
    [Fact]
    public async Task SearchKnowledgeBase_ShouldReturnSearchResults()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var queryString = "test query";
        var topK = 5;
        var allowedVisibility = new List<string> { "public" };

        var mockSearchResults = new List<KnowledgeSearchResultDto>
        {
            new KnowledgeSearchResultDto
            {
                Metadata = new Dictionary<string, object> { { "original_id", "M001" }, { "name", "Member 1" }, { "content_type", "Member" } },
                Summary = "Summary of Member 1",
                Score = 0.9
            },
            new KnowledgeSearchResultDto
            {
                Metadata = new Dictionary<string, object> { { "original_id", "E001" }, { "name", "Event 1" }, { "content_type", "Event" } },
                Summary = "Summary of Event 1",
                Score = 0.8
            }
        };

        var apiResponse = new { Results = mockSearchResults };
        var jsonResponse = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri == new Uri("http://localhost:8000/api/v1/search")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var results = await _knowledgeService.SearchKnowledgeBase(familyId, queryString, topK, allowedVisibility);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(2);
        results[0].Metadata["original_id"].ToString().Should().Be("M001");
        results[0].Summary.Should().Be("Summary of Member 1");
        results[0].Score.Should().Be(0.9);
        results[1].Metadata["original_id"].ToString().Should().Be("E001");
        results[1].Summary.Should().Be("Summary of Event 1");
        results[1].Score.Should().Be(0.8);

        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri == new Uri("http://localhost:8000/api/v1/search") &&
                req.Content != null &&
                VerifySearchRequestContent(req.Content, familyId, queryString, topK, allowedVisibility)
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    // --- Helper for verifying Search Request content ---
    private bool VerifySearchRequestContent(HttpContent content, Guid familyId, string queryString, int topK, List<string> allowedVisibility)
    {
        if (content == null) return false;
        var json = content.ReadAsStringAsync().Result;
        var requestBody = JsonSerializer.Deserialize<JsonElement>(json);

        requestBody.GetProperty("family_id").GetString().Should().Be(familyId.ToString());
        requestBody.GetProperty("query").GetString().Should().Be(queryString);
        requestBody.GetProperty("top_k").GetInt32().Should().Be(topK);
        requestBody.GetProperty("allowed_visibility").GetArrayLength().Should().Be(allowedVisibility.Count);
        requestBody.GetProperty("allowed_visibility")[0].GetString().Should().Be(allowedVisibility[0]);

        return true;
    }
}
