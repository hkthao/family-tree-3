#nullable disable

using System.Net;
using System.Text;
using System.Text.Json;
using backend.Application.Common.Models.LLMGateway;
using backend.Application.UnitTests.Common; // For TestBase
using backend.Infrastructure.Services.LLMGateway;
using FluentAssertions; // Fluent Assertions
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

// Inherit from TestBase
public class LLMGatewayServiceTests : TestBase
{
    // _mockHttpMessageHandler and _httpClient are local to this test class for direct mocking of HTTP responses
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly LLMGatewayService _llmGatewayService;
    private readonly Mock<ILogger<LLMGatewayService>> _mockLogger;

    public LLMGatewayServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<LLMGatewayService>>();

        // Setup Configuration mock for BaseUrl
        _mockConfiguration.Setup(c => c["LLMGatewayService:BaseUrl"])
                          .Returns("http://test-llm-gateway/");

        // Pass the mocked logger to the service
        _llmGatewayService = new LLMGatewayService(_httpClient, _mockConfiguration.Object, _mockLogger.Object);
    }

    // Helper method to set up HttpClient mock response
    private void SetupMockResponse(HttpStatusCode statusCode, string content)
    {
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            })
            .Verifiable();
    }

    [Fact]
    public async Task GetChatCompletionAsync_ShouldReturnSuccess_OnValidResponse()
    {
        // Arrange
        var request = new LLMChatCompletionRequest
        {
            Model = "ollama:llama2",
            Messages = new List<LLMChatCompletionMessage> { new LLMChatCompletionMessage { Role = "user", Content = "Hello" } }
        };

        var expectedResponse = new LLMChatCompletionResponse
        {
            Id = "test-id",
            Object = "chat.completion",
            Choices = new List<LLMChatCompletionChoice>
            {
                new LLMChatCompletionChoice
                {
                    Index = 0,
                    Message = new LLMChatCompletionMessage { Role = "assistant", Content = "Hi there!" },
                    FinishReason = "stop"
                }
            }
        };
        SetupMockResponse(HttpStatusCode.OK, JsonSerializer.Serialize(expectedResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));

        // Act
        var result = await _llmGatewayService.GetChatCompletionAsync(request);

        // Assert (using Fluent Assertions)
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        var actualResponse = (LLMChatCompletionResponse)result.Value; // Explicit cast

        actualResponse.Id.Should().Be(expectedResponse.Id);
        actualResponse.Choices[0].Message.Content.Should().Be(expectedResponse.Choices[0].Message.Content);
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString().EndsWith("/v1/chat/completions")
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetChatCompletionAsync_ShouldReturnFailure_OnApiError()
    {
        // Arrange
        var request = new LLMChatCompletionRequest
        {
            Model = "ollama:llama2",
            Messages = new List<LLMChatCompletionMessage> { new LLMChatCompletionMessage { Role = "user", Content = "Hello" } }
        };
        SetupMockResponse(HttpStatusCode.InternalServerError, "Error from API");

        // Act
        var result = await _llmGatewayService.GetChatCompletionAsync(request);

        // Assert (using Fluent Assertions)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("LLM Gateway Chat API call failed");
    }

    [Fact]
    public async Task GetEmbeddingsAsync_ShouldReturnSuccess_OnValidResponse()
    {
        // Arrange
        var request = new LLMEmbeddingRequest
        {
            Model = "ollama:nomic-embed-text",
            Input = "Test sentence"
        };

        var expectedResponse = new LLMEmbeddingResponse
        {
            Object = "list",
            Data = new List<LLMEmbeddingData>
            {
                new LLMEmbeddingData
                {
                    Object = "embedding",
                    Embedding = new List<float> { 0.1f, 0.2f, 0.3f },
                    Index = 0
                }
            },
            Model = "ollama:nomic-embed-text",
            Usage = new LLMEmbeddingUsage { PromptTokens = 5, TotalTokens = 5 }
        };
        SetupMockResponse(HttpStatusCode.OK, JsonSerializer.Serialize(expectedResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));

        // Act
        var result = await _llmGatewayService.GetEmbeddingsAsync(request);

        // Assert (using Fluent Assertions)
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        var actualResponse = (LLMEmbeddingResponse)result.Value; // Explicit cast

        actualResponse.Model.Should().Be(expectedResponse.Model);
        actualResponse.Data[0].Embedding.Should().HaveCount(expectedResponse.Data[0].Embedding.Count);
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString().EndsWith("/v1/embeddings")
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetEmbeddingsAsync_ShouldReturnFailure_OnApiError()
    {
        // Arrange
        var request = new LLMEmbeddingRequest
        {
            Model = "ollama:nomic-embed-text",
            Input = "Test sentence"
        };
        SetupMockResponse(HttpStatusCode.BadRequest, "Invalid embedding request");

        // Act
        var result = await _llmGatewayService.GetEmbeddingsAsync(request);

        // Assert (using Fluent Assertions)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("LLM Gateway Embeddings API call failed");
    }
}