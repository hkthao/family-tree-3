using Moq;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using McpServer.Config;
using Moq.Protected;
using McpServer.Services.Integrations;

namespace McpServer.UnitTests;

public class FamilyTreeBackendServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<IOptions<FamilyTreeBackendSettings>> _mockSettings;
    private readonly Mock<ILogger<FamilyTreeBackendService>> _mockLogger;
    private readonly FamilyTreeBackendService _service;

    public FamilyTreeBackendServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5000/")
        };

        _mockSettings = new Mock<IOptions<FamilyTreeBackendSettings>>();
        _mockSettings.Setup(s => s.Value).Returns(new FamilyTreeBackendSettings { BaseUrl = "http://localhost:5000" });

        _mockLogger = new Mock<ILogger<FamilyTreeBackendService>>();

        _service = new FamilyTreeBackendService(_httpClient, _mockSettings.Object, _mockLogger.Object);
    }

    private HttpResponseMessage CreateHttpResponse(HttpStatusCode statusCode, object? content = null)
    {
        var response = new HttpResponseMessage(statusCode);
        if (content != null)
        {
            response.Content = new StringContent(JsonSerializer.Serialize(content));
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        }
        return response;
    }

    [Fact]
    public async Task GetMembersAsync_ReturnsMembers_OnSuccess()
    {
        // Arrange
        var jwtToken = "test_jwt";
        var expectedMembers = new List<MemberDto>
        {
            new MemberDto { Id = Guid.NewGuid(), FullName = "John Doe" },
            new MemberDto { Id = Guid.NewGuid(), FullName = "Jane Doe" }
        };
        var httpResponse = CreateHttpResponse(HttpStatusCode.OK, expectedMembers);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetMembersAsync(jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedMembers.Count, result.Count);
        Assert.Equal(expectedMembers[0].FullName, result[0].FullName);
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri!.ToString().Contains("api/member") &&
                req.Headers.Authorization!.Parameter == jwtToken),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetMembersAsync_ReturnsNull_OnHttpRequestException()
    {
        // Arrange
        var jwtToken = "test_jwt";
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Simulated network error"));

        // Act
        var result = await _service.GetMembersAsync(jwtToken);

        // Assert
        Assert.Null(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error calling Family Tree backend to get members.")),
                It.IsAny<HttpRequestException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetMembersAsync_ReturnsNull_OnJsonException()
    {
        // Arrange
        var jwtToken = "test_jwt";
        var httpResponse = CreateHttpResponse(HttpStatusCode.OK, "invalid json"); // Invalid JSON
        httpResponse.Content = new StringContent("invalid json"); // Override with invalid JSON

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetMembersAsync(jwtToken);

        // Assert
        Assert.Null(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error deserializing members from Family Tree backend.")),
                It.IsAny<JsonException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
