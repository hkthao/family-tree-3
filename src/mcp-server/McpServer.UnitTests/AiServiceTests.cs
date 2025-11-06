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
    private readonly Mock<ToolInteractionHandler> _mockToolInteractionHandler; // Add mock for ToolInteractionHandler
    private readonly Mock<IAiProvider> _mockAiProvider; // Add mock for IAiProvider
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly AiService _aiService;

    public AiServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockAiProvider = new Mock<IAiProvider>(); // Instantiate mock for IAiProvider first
        _mockAiProviderFactory = new Mock<AiProviderFactory>(MockBehavior.Strict, _mockServiceProvider.Object);
        _mockAiProviderFactory.Setup(f => f.GetProvider(It.IsAny<string>())).Returns(_mockAiProvider.Object!); // Setup AiProviderFactory
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



        _mockToolInteractionHandler = new Mock<ToolInteractionHandler>(MockBehavior.Strict, _mockAiProvider.Object, _mockToolExecutor.Object, new Mock<ILogger<ToolInteractionHandler>>().Object); // Instantiate mock for ToolInteractionHandler

        _aiService = new AiService(
            _mockAiProviderFactory.Object,
            _mockLogger.Object,
            _mockConfiguration.Object,
            _mockToolExecutor.Object,
            _mockToolInteractionHandler.Object); // Pass mock to AiService constructor
    }

    [Fact]
    public async Task GetAiResponseStreamAsync_DirectTextResponse_ReturnsText()
    {
        // Arrange
        var prompt = "Hello AI";
        var expectedText = "Hello user!";

        // Override the default behavior for _mockToolInteractionHandler for this specific test
        _mockToolInteractionHandler.Setup(h => h.HandleToolInteractionAsync(
            It.IsAny<string>(),
            It.IsAny<string>()))
            .Returns(GetAsyncEnumerable(expectedText));

        // Act
        var resultChunks = new List<string>();
        await foreach (var chunk in _aiService.GetAiResponseStreamAsync(prompt, null))
        {
            resultChunks.Add(chunk);
        }

        // Assert
        Assert.Single(resultChunks);
        Assert.Equal(expectedText, resultChunks.First());
        _mockToolInteractionHandler.Verify(h => h.HandleToolInteractionAsync(
            prompt,
            null),
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

    private static async IAsyncEnumerable<string> GetAsyncEnumerable(params string[] parts)
    {
        foreach (var part in parts)
        {
            await Task.Yield(); // To make it truly asynchronous
            yield return part;
        }
    }

    private static async IAsyncEnumerable<AiResponsePart> GetAiResponsePartAsyncEnumerable(params AiResponsePart[] parts)
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
        var finalAiResponseText = "Thông tin gia đình ABC đã được tìm thấy.";

        // Setup ToolInteractionHandler to return the expected sequence of events
        _mockToolInteractionHandler.Setup(h => h.HandleToolInteractionAsync(
            It.IsAny<string>(),
            It.IsAny<string>()))
            .Returns(GetAsyncEnumerable(finalAiResponseText));

        // Act
        var resultChunks = new List<string>();
        await foreach (var chunk in _aiService.GetAiResponseStreamAsync(prompt, jwtToken))
        {
            resultChunks.Add(chunk);
        }

        // Assert
        Assert.Single(resultChunks);
        Assert.Equal(finalAiResponseText, resultChunks.First());

        _mockToolInteractionHandler.Verify(h => h.HandleToolInteractionAsync(
            prompt,
            jwtToken
        ), Times.Once);
    }
}