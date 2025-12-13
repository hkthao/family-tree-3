using System.Net;
using backend.Application.AI.DTOs;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AppSetting;
using backend.Infrastructure.Auth;
using backend.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace backend.Infrastructure.UnitTests.Services;

public class AiChatServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IOptions<N8nSettings>> _n8nSettingsMock;
    private readonly Mock<ILogger<AiChatService>> _loggerMock;
    private readonly Mock<IJwtHelperFactory> _jwtHelperFactoryMock;
    private readonly Mock<IJwtHelper> _jwtHelperMock;
    private readonly AiChatService _chatAiService;

    public AiChatServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _n8nSettingsMock = new Mock<IOptions<N8nSettings>>();
        _loggerMock = new Mock<ILogger<AiChatService>>();
        _jwtHelperFactoryMock = new Mock<IJwtHelperFactory>();
        _jwtHelperMock = new Mock<IJwtHelper>();

        // Setup N8nSettings
        _n8nSettingsMock.Setup(x => x.Value).Returns(new N8nSettings
        {
            Chat = new ChatSettings { WebhookUrl = "http://test.n8n/chat" },
            JwtSecret = "supersecretkey"
        });

        _jwtHelperFactoryMock.Setup(x => x.Create(It.IsAny<string>())).Returns(_jwtHelperMock.Object);
        _jwtHelperMock.Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<DateTime>())).Returns("mocked_jwt_token");

        _chatAiService = new AiChatService(
            _httpClientFactoryMock.Object,
            _n8nSettingsMock.Object,
            _loggerMock.Object,
            _jwtHelperFactoryMock.Object);
    }

    private HttpClient CreateMockHttpClient(HttpResponseMessage responseMessage)
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                It.IsAny<HttpRequestMessage>(),
                It.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        return new HttpClient(mockHttpMessageHandler.Object) { BaseAddress = new Uri("http://test.n8n/") };
    }

    [Fact]
    public async Task CallChatWebhookAsync_ShouldReturnFailure_WhenWebhookUrlIsNotConfigured()
    {
        // Arrange
        _n8nSettingsMock.Setup(x => x.Value).Returns(new N8nSettings
        {
            Chat = new ChatSettings { WebhookUrl = "" }, // Empty webhook URL
            JwtSecret = "supersecretkey"
        });

        var request = new ChatRequest { SessionId = "s1", ChatInput = "hello" };

        // Act
        var result = await _chatAiService.CallChatWebhookAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("n8n chat integration is not configured.");
        _loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("n8n chat webhook URL is not configured.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task CallChatWebhookAsync_ShouldReturnSuccess_OnSuccessfulApiCall()
    {
        // Arrange
        var expectedOutput = "Hello from AI";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"{{\"output\":\"{expectedOutput}\"}}")
        };
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(CreateMockHttpClient(httpResponse));

        var request = new ChatRequest { SessionId = "s1", ChatInput = "hello" };

        // Act
        var result = await _chatAiService.CallChatWebhookAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Output.Should().Be(expectedOutput);
    }

    [Fact]
    public async Task CallChatWebhookAsync_ShouldReturnFailure_OnUnsuccessfulApiCall()
    {
        // Arrange
        var errorContent = "Internal server error from n8n";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent(errorContent)
        };
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(CreateMockHttpClient(httpResponse));

        var request = new ChatRequest { SessionId = "s1", ChatInput = "hello" };

        // Act
        var result = await _chatAiService.CallChatWebhookAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to call n8n chat webhook.");
        result.Error.Should().Contain(errorContent);
    }

    [Fact]
    public async Task CallChatWebhookAsync_ShouldReturnFailure_OnEmptyResponseContent()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("") // Empty content
        };
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(CreateMockHttpClient(httpResponse));

        var request = new ChatRequest { SessionId = "s1", ChatInput = "hello" };

        // Act
        var result = await _chatAiService.CallChatWebhookAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Received empty response.");
    }

    [Fact]
    public async Task CallChatWebhookAsync_ShouldReturnFailure_OnInvalidJsonResponse()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("invalid json") // Invalid JSON
        };
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(CreateMockHttpClient(httpResponse));

        var request = new ChatRequest { SessionId = "s1", ChatInput = "hello" };

        // Act
        var result = await _chatAiService.CallChatWebhookAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to deserialize chat response.");
    }

    [Fact]
    public async Task CallChatWebhookAsync_ShouldIncludeJwtToken_WhenJwtSecretIsConfigured()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"{{\"output\":\"AI response\"}}")
        };
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(CreateMockHttpClient(httpResponse));

        var request = new ChatRequest { SessionId = "s1", ChatInput = "hello" };

        // Act
        var result = await _chatAiService.CallChatWebhookAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _jwtHelperFactoryMock.Verify(x => x.Create("supersecretkey"), Times.Once);
        _jwtHelperMock.Verify(x => x.GenerateToken("s1", It.IsAny<DateTime>()), Times.Once);
        // Additional verification would be needed to check the actual header in the outgoing request,
        // which might require more complex mocking of HttpClient or handler.
    }

    [Fact]
    public async Task CallChatWebhookAsync_ShouldNotIncludeJwtToken_WhenJwtSecretIsNotConfigured()
    {
        // Arrange
        _n8nSettingsMock.Setup(x => x.Value).Returns(new N8nSettings
        {
            Chat = new ChatSettings { WebhookUrl = "http://test.n8n/chat" },
            JwtSecret = "" // No JWT secret
        });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"{{\"output\":\"AI response\"}}")
        };
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(CreateMockHttpClient(httpResponse));

        var request = new ChatRequest { SessionId = "s1", ChatInput = "hello" };

        // Act
        var result = await _chatAiService.CallChatWebhookAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _jwtHelperFactoryMock.Verify(x => x.Create(It.IsAny<string>()), Times.Never);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("N8nSettings.JwtSecret is not configured.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }
}
