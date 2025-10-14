using Xunit;
using FluentAssertions;
using Moq;
using backend.Application.AI.Chat.Queries;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using backend.Application.AI.VectorStore;
using Microsoft.Extensions.Logging;
using backend.Domain.Entities; // Added for TextChunk
using backend.Application.UnitTests.Common;

namespace backend.Application.UnitTests.AI.Chat.Queries;

public class ChatWithAssistantQueryHandlerTests : TestBase
{
    private readonly Mock<IChatProviderFactory> _mockChatProviderFactory;
    private readonly Mock<IEmbeddingProviderFactory> _mockEmbeddingProviderFactory;
    private readonly Mock<IVectorStoreFactory> _mockVectorStoreFactory;
    private readonly AIChatSettings _chatSettings;
    private readonly EmbeddingSettings _embeddingSettings;
    private readonly VectorStoreSettings _vectorStoreSettings;
    private readonly Mock<ILogger<ChatWithAssistantQueryHandler>> _mockLogger;
    private readonly ChatWithAssistantQueryHandler _handler;

    public ChatWithAssistantQueryHandlerTests()
    {
        _mockChatProviderFactory = new Mock<IChatProviderFactory>();
        _mockEmbeddingProviderFactory = new Mock<IEmbeddingProviderFactory>();
        _mockVectorStoreFactory = new Mock<IVectorStoreFactory>();
        _mockLogger = new Mock<ILogger<ChatWithAssistantQueryHandler>>();

        _chatSettings = new AIChatSettings
        {
            Provider = ChatAIProvider.Gemini.ToString(),
            ScoreThreshold = 0.7f
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

        _handler = new ChatWithAssistantQueryHandler(
            _mockChatProviderFactory.Object,
            _mockEmbeddingProviderFactory.Object,
            _mockVectorStoreFactory.Object,
            _chatSettings,
            _embeddingSettings,
            _vectorStoreSettings,
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
            .ReturnsAsync(Result<float[]>.Success(new float[] { 0.1f, 0.2f }));
        _mockEmbeddingProviderFactory.Setup(f => f.GetProvider(It.IsAny<EmbeddingAIProvider>()))
            .Returns(mockEmbeddingProvider.Object);

        var mockVectorStore = new Mock<IVectorStore>();
        mockVectorStore.Setup(s => s.QueryAsync(It.IsAny<float[]>(), It.IsAny<int>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TextChunk>
            {
                new TextChunk { Content = "Relevant context 1", Score = 0.8f },
                new TextChunk { Content = "Relevant context 2", Score = 0.9f }
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
        mockVectorStore.Verify(s => s.QueryAsync(It.IsAny<float[]>(), _vectorStoreSettings.TopK, It.IsAny<Dictionary<string, string>>(), cancellationToken), Times.Once);
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
            .ReturnsAsync(Result<float[]>.Success(new float[] { 0.1f, 0.2f }));
        _mockEmbeddingProviderFactory.Setup(f => f.GetProvider(It.IsAny<EmbeddingAIProvider>()))
            .Returns(mockEmbeddingProvider.Object);

        var mockVectorStore = new Mock<IVectorStore>();
        mockVectorStore.Setup(s => s.QueryAsync(It.IsAny<float[]>(), It.IsAny<int>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TextChunk>
            {
                new TextChunk { Content = "Irrelevant context", Score = 0.5f } // Score below threshold
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
            .ReturnsAsync(Result<float[]>.Failure("Embedding error"));
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