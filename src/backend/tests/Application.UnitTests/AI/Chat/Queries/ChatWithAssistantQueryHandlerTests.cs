using backend.Application.AI.Chat.Queries;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.AI.VectorStore;
using backend.Application.Common.Models.AppSetting;
using backend.Application.UnitTests.Common;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.AI.Chat.Queries;

public class ChatWithAssistantQueryHandlerTests : TestBase
{
    private readonly Mock<IChatProviderFactory> _mockChatProviderFactory;
    private readonly Mock<IEmbeddingProviderFactory> _mockEmbeddingProviderFactory;
    private readonly Mock<IVectorStoreFactory> _mockVectorStoreFactory;
    private readonly Mock<IConfigProvider> _mockConfigProvider;
    private readonly Mock<ILogger<ChatWithAssistantQueryHandler>> _mockLogger;
    private readonly AIChatSettings _chatSettings;
    private readonly EmbeddingSettings _embeddingSettings;
    private readonly VectorStoreSettings _vectorStoreSettings;
    private readonly ChatWithAssistantQueryHandler _handler;

    public ChatWithAssistantQueryHandlerTests()
    {
        _mockChatProviderFactory = new Mock<IChatProviderFactory>();
        _mockEmbeddingProviderFactory = new Mock<IEmbeddingProviderFactory>();
        _mockVectorStoreFactory = new Mock<IVectorStoreFactory>();
        _mockConfigProvider = new Mock<IConfigProvider>();
        _mockLogger = new Mock<ILogger<ChatWithAssistantQueryHandler>>();

        _chatSettings = new AIChatSettings
        {
            Provider = ChatAIProvider.Gemini.ToString(),
            ScoreThreshold = 70 // Changed from 0.7f to 70
        };
        _embeddingSettings = new EmbeddingSettings
        {
            Provider = EmbeddingAIProvider.OpenAI.ToString()
        };
        _vectorStoreSettings = new VectorStoreSettings
        {
            Provider = VectorStoreProviderType.InMemory.ToString(),
            TopK = 3
        };

        _mockConfigProvider.Setup(x => x.GetSection<AIChatSettings>()).Returns(_chatSettings);
        _mockConfigProvider.Setup(x => x.GetSection<EmbeddingSettings>()).Returns(_embeddingSettings);
        _mockConfigProvider.Setup(x => x.GetSection<VectorStoreSettings>()).Returns(_vectorStoreSettings);

        _handler = new ChatWithAssistantQueryHandler(
            _mockChatProviderFactory.Object,
            _mockEmbeddingProviderFactory.Object,
            _mockVectorStoreFactory.Object,
            _mockConfigProvider.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithChatResponse_WhenRelevantContextFound()
    {
        // Arrange
        var query = new ChatWithAssistantQuery("Test message", "session1");
        var cancellationToken = CancellationToken.None;

        var mockEmbeddingProvider = new Mock<IEmbeddingProvider>();
        mockEmbeddingProvider.Setup(p => p.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<double[]>.Success(new double[] { 0.1, 0.2 }));
        _mockEmbeddingProviderFactory.Setup(f => f.GetProvider(It.IsAny<EmbeddingAIProvider>()))
            .Returns(mockEmbeddingProvider.Object);

        var mockVectorStore = new Mock<IVectorStore>();
        mockVectorStore.Setup(s => s.QueryAsync(It.IsAny<double[]>(), It.IsAny<int>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VectorStoreQueryResult>
            {
                new VectorStoreQueryResult { Id = "1", Content = "Relevant context 1", Score = 0.8, Embedding = new List<double>() },
                new VectorStoreQueryResult { Id = "2", Content = "Relevant context 2", Score = 0.9, Embedding = new List<double>() }
            });
        _mockVectorStoreFactory.Setup(f => f.CreateVectorStore(It.IsAny<VectorStoreProviderType>()))
            .Returns(mockVectorStore.Object);

        var mockChatProvider = new Mock<IChatProvider>();
        mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync("AI generated response");
        _mockChatProviderFactory.Setup(f => f.GetProvider(It.IsAny<ChatAIProvider>()))
            .Returns(mockChatProvider.Object);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Response.Should().Be("AI generated response");
        result.Value.SessionId.Should().Be(query.SessionId);
        result.Value.Model.Should().Be(_chatSettings.Provider.ToString());
        _mockEmbeddingProviderFactory.Verify(f => f.GetProvider(EmbeddingAIProvider.OpenAI), Times.Once);
        mockEmbeddingProvider.Verify(p => p.GenerateEmbeddingAsync(query.UserMessage, cancellationToken), Times.Once);
        _mockVectorStoreFactory.Verify(f => f.CreateVectorStore(VectorStoreProviderType.InMemory), Times.Once);
        mockVectorStore.Verify(s => s.QueryAsync(It.IsAny<double[]>(), _vectorStoreSettings.TopK, It.IsAny<Dictionary<string, string>>(), cancellationToken), Times.Once);
        _mockChatProviderFactory.Verify(f => f.GetProvider(ChatAIProvider.Gemini), Times.Once);
        mockChatProvider.Verify(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithFallbackResponse_WhenNoRelevantContextFound()
    {
        // Arrange
        var query = new ChatWithAssistantQuery("Test message", "session1");
        var cancellationToken = CancellationToken.None;

        var mockEmbeddingProvider = new Mock<IEmbeddingProvider>();
        mockEmbeddingProvider.Setup(p => p.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<double[]>.Success(new double[] { 0.1, 0.2 }));
        _mockEmbeddingProviderFactory.Setup(f => f.GetProvider(It.IsAny<EmbeddingAIProvider>()))
            .Returns(mockEmbeddingProvider.Object);

        var mockVectorStore = new Mock<IVectorStore>();
        mockVectorStore.Setup(s => s.QueryAsync(It.IsAny<double[]>(), It.IsAny<int>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VectorStoreQueryResult>
            {
                new VectorStoreQueryResult { Id = "3", Content = "Irrelevant context", Score = 0.5, Embedding = new List<double>() } // Score below threshold
            });
        _mockVectorStoreFactory.Setup(f => f.CreateVectorStore(It.IsAny<VectorStoreProviderType>()))
            .Returns(mockVectorStore.Object);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Response.Should().Contain("Tôi không tìm thấy thông tin liên quan");
        result.Value.SessionId.Should().Be(query.SessionId);
        result.Value.Model.Should().Be("Fallback");
        _mockChatProviderFactory.Verify(f => f.GetProvider(It.IsAny<ChatAIProvider>()), Times.Never); // Chat provider should not be called
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmbeddingGenerationFails()
    {
        // Arrange
        var query = new ChatWithAssistantQuery("Test message", "session1");
        var cancellationToken = CancellationToken.None;

        var mockEmbeddingProvider = new Mock<IEmbeddingProvider>();
        mockEmbeddingProvider.Setup(p => p.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<double[]>.Failure("Embedding error"));
        _mockEmbeddingProviderFactory.Setup(f => f.GetProvider(It.IsAny<EmbeddingAIProvider>()))
            .Returns(mockEmbeddingProvider.Object);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to generate embedding: Embedding error");
        _mockVectorStoreFactory.Verify(f => f.CreateVectorStore(It.IsAny<VectorStoreProviderType>()), Times.Never);
        _mockChatProviderFactory.Verify(f => f.GetProvider(It.IsAny<ChatAIProvider>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        var query = new ChatWithAssistantQuery("Test message", "session1");
        var cancellationToken = CancellationToken.None;

        var mockEmbeddingProvider = new Mock<IEmbeddingProvider>();
        mockEmbeddingProvider.Setup(p => p.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Simulated exception"));
        _mockEmbeddingProviderFactory.Setup(f => f.GetProvider(It.IsAny<EmbeddingAIProvider>()))
            .Returns(mockEmbeddingProvider.Object);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to generate chat response: Simulated exception");
    }
}