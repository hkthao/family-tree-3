using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using McpServer.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using System;
using Microsoft.Extensions.Options;
using McpServer.Config;

namespace McpServer.UnitTests;

public class AiServiceTests
{
    private readonly Mock<AiProviderFactory> _mockAiProviderFactory;
    private readonly Mock<ILogger<AiService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ToolExecutor> _mockToolExecutor;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly AiService _aiService;

    public AiServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockAiProviderFactory = new Mock<AiProviderFactory>(MockBehavior.Strict, _mockServiceProvider.Object);
        _mockLogger = new Mock<ILogger<AiService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c["DefaultAiProvider"]).Returns("Gemini");

        // Create mocks for ToolExecutor's dependencies
        var mockHttpClient = new Mock<HttpClient>();
        var mockFamilyTreeBackendSettings = new Mock<IOptions<FamilyTreeBackendSettings>>();
        mockFamilyTreeBackendSettings.Setup(o => o.Value).Returns(new FamilyTreeBackendSettings { BaseUrl = "http://localhost:5000" });
        var mockFamilyTreeBackendServiceLogger = new Mock<ILogger<FamilyTreeBackendService>>();

        var mockFamilyTreeBackendService = new Mock<FamilyTreeBackendService>(MockBehavior.Strict, mockHttpClient.Object, mockFamilyTreeBackendSettings.Object, mockFamilyTreeBackendServiceLogger.Object);
        var mockToolExecutorLogger = new Mock<ILogger<ToolExecutor>>();

        _mockToolExecutor = new Mock<ToolExecutor>(MockBehavior.Strict, mockFamilyTreeBackendService.Object, mockToolExecutorLogger.Object);

        _aiService = new AiService(
            _mockAiProviderFactory.Object,
            _mockLogger.Object,
            _mockConfiguration.Object,
            _mockToolExecutor.Object);
    }

    [Fact]
    public async Task GetAiResponseStreamAsync_DirectTextResponse_ReturnsText()
    {
        // Arrange
        var prompt = "Hello AI";
        var expectedText = "Hello user!";
        var mockAiProvider = new Mock<IAiProvider>();
        mockAiProvider.Setup(p => p.GenerateResponseStreamAsync(
            It.IsAny<string>(),
            It.IsAny<List<AiToolDefinition>>(),
            It.IsAny<List<AiToolResult>>())
        ).Returns(GetAsyncEnumerable(new AiTextResponsePart(expectedText)));

        _mockAiProviderFactory.Setup(f => f.GetProvider("Gemini")).Returns(mockAiProvider.Object);

        // Act
        var resultChunks = new List<string>();
        await foreach (var chunk in _aiService.GetAiResponseStreamAsync(prompt, null))
        {
            resultChunks.Add(chunk);
        }

        // Assert
        Assert.Single(resultChunks);
        Assert.Equal(expectedText, resultChunks.First());
        mockAiProvider.Verify(p => p.GenerateResponseStreamAsync(
            prompt,
            It.IsAny<List<AiToolDefinition>>(),
            It.IsAny<List<AiToolResult>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAiResponseStreamAsync_InvalidAiProvider_ReturnsError()
    {
        // Arrange
        var prompt = "Hello AI";
        var invalidProviderName = "InvalidProvider";
        _mockAiProviderFactory.Setup(f => f.GetProvider(invalidProviderName))
            .Throws(new ArgumentException($"Invalid AI provider: {invalidProviderName}"));

        // Act
        var resultChunks = new List<string>();
        await foreach (var chunk in _aiService.GetAiResponseStreamAsync(prompt, null, invalidProviderName))
        {
            resultChunks.Add(chunk);
        }

        // Assert
        Assert.Single(resultChunks);
        Assert.Contains($"Error: Invalid AI provider '{invalidProviderName}'.", resultChunks.First());
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains($"Invalid AI provider specified: {invalidProviderName}")),
                It.IsAny<ArgumentException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static async IAsyncEnumerable<AiResponsePart> GetAsyncEnumerable(params AiResponsePart[] parts)
    {
        foreach (var part in parts)
        {
            await Task.Yield(); // To make it truly asynchronous
            yield return part;
        }
    }

    [Fact]
    public async Task GetAiResponseStreamAsync_SingleToolCall_ExecutesToolAndReturnsText()
    {
        // Arrange
        var prompt = "Tìm thông tin gia đình có ID là 1a955fff-ce01-422f-8bb3-02ab14e8ec47.";
        var jwtToken = "test_jwt";
        var familyId = Guid.Parse("1a955fff-ce01-422f-8bb3-02ab14e8ec47");
        var toolCallId = "call_id_123";
        var toolName = "get_family_details";
        var toolArgs = JsonSerializer.Serialize(new { id = familyId.ToString() });
        var toolResultContent = JsonSerializer.Serialize(new { Name = "Gia đình ABC", History = "Lịch sử gia đình ABC" });
        var finalAiResponseText = "Thông tin gia đình ABC đã được tìm thấy.";

        var aiToolCall = new AiToolCall(toolCallId, toolName, toolArgs);
        var aiToolResult = new AiToolResult(toolCallId, toolResultContent);

        var mockAiProvider = new Mock<IAiProvider>();

        // Setup for the first call to GenerateResponseStreamAsync (LLM returns tool call)
        mockAiProvider.SetupSequence(p => p.GenerateResponseStreamAsync(
            It.IsAny<string>(),
            It.IsAny<List<AiToolDefinition>>(),
            It.IsAny<List<AiToolResult>>() // Expect no tool results in the first call
        ))
        .Returns(GetAsyncEnumerable(new AiToolCallResponsePart(new List<AiToolCall> { aiToolCall })));

        // Setup for the second call to GenerateResponseStreamAsync (LLM returns final text after tool result)
        mockAiProvider.SetupSequence(p => p.GenerateResponseStreamAsync(
            It.IsAny<string>(),
            It.IsAny<List<AiToolDefinition>>(),
            It.Is<List<AiToolResult>>(tr => tr != null && tr.Any(r => r.ToolCallId == toolCallId && r.Content == toolResultContent)) // Expect tool result in the second call
        ))
        .Returns(GetAsyncEnumerable(new AiTextResponsePart(finalAiResponseText)));

        _mockAiProviderFactory.Setup(f => f.GetProvider("Gemini")).Returns(mockAiProvider.Object);

        _mockToolExecutor.Setup(te => te.ExecuteToolCallAsync(
            It.Is<AiToolCall>(tc => tc.Id == toolCallId && tc.FunctionName == toolName && tc.FunctionArgs == toolArgs),
            jwtToken
        )).ReturnsAsync(aiToolResult);

        // Act
        var resultChunks = new List<string>();
        await foreach (var chunk in _aiService.GetAiResponseStreamAsync(prompt, jwtToken))
        {
            resultChunks.Add(chunk);
        }

        // Assert
        Assert.Single(resultChunks);
        Assert.Equal(finalAiResponseText, resultChunks.First());

        _mockToolExecutor.Verify(te => te.ExecuteToolCallAsync(
            It.Is<AiToolCall>(tc => tc.Id == toolCallId && tc.FunctionName == toolName && tc.FunctionArgs == toolArgs),
            jwtToken
        ), Times.Once);

        mockAiProvider.Verify(p => p.GenerateResponseStreamAsync(
            prompt,
            It.IsAny<List<AiToolDefinition>>(),
            It.IsAny<List<AiToolResult>>()),
            Times.Exactly(2)); // Called twice: once for tool call, once for final response
    }
}
