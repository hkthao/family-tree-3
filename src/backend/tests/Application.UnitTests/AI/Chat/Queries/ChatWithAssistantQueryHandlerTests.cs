using backend.Application.AI.Chat;
using backend.Application.AI.Chat.Queries;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Application.UnitTests.Common;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using backend.Application.AI.VectorStore; // Added for VectorStoreResult
using FluentAssertions;

namespace backend.Application.UnitTests.AI.Chat.Queries;

public class ChatWithAssistantQueryHandlerTests : TestBase
{
    private readonly Mock<IChatProviderFactory> _mockChatProviderFactory;
    private readonly Mock<IEmbeddingProviderFactory> _mockEmbeddingProviderFactory;
    private readonly Mock<IVectorStoreFactory> _mockVectorStoreFactory;
    private readonly Mock<IConfigProvider> _mockConfigProvider;
    private readonly Mock<ILogger<ChatWithAssistantQueryHandler>> _mockLogger;
    private readonly ChatWithAssistantQueryHandler _handler;

    public ChatWithAssistantQueryHandlerTests()
    {
        _mockChatProviderFactory = new Mock<IChatProviderFactory>();
        _mockEmbeddingProviderFactory = new Mock<IEmbeddingProviderFactory>();
        _mockVectorStoreFactory = new Mock<IVectorStoreFactory>();
        _mockConfigProvider = new Mock<IConfigProvider>();
        _mockLogger = new Mock<ILogger<ChatWithAssistantQueryHandler>>();

        _handler = new ChatWithAssistantQueryHandler(
            _mockChatProviderFactory.Object,
            _mockEmbeddingProviderFactory.Object,
            _mockVectorStoreFactory.Object,
            _mockConfigProvider.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnChatResponse_WhenContextFound()
    {
        // Arrange
        var userMessage = "Test message";
        var sessionId = "test-session";
        var query = new ChatWithAssistantQuery(userMessage, sessionId);

        var embeddingSettings = new EmbeddingSettings { Provider = "OpenAI" };
        var vectorStoreSettings = new VectorStoreSettings { Provider = "Pinecone", TopK = 3 };
        var chatSettings = new AIChatSettings { Provider = "Gemini", ScoreThreshold = 70 };

        _mockConfigProvider.Setup(x => x.GetSection<EmbeddingSettings>()).Returns(embeddingSettings);
        _mockConfigProvider.Setup(x => x.GetSection<VectorStoreSettings>()).Returns(vectorStoreSettings);
        _mockConfigProvider.Setup(x => x.GetSection<AIChatSettings>()).Returns(chatSettings);

        var mockEmbeddingProvider = new Mock<IEmbeddingProvider>();
        mockEmbeddingProvider.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<double[]>.Success(new double[] { 0.1, 0.2, 0.3 })); // Changed to double[]
        _mockEmbeddingProviderFactory.Setup(x => x.GetProvider(It.IsAny<EmbeddingAIProvider>())).Returns(mockEmbeddingProvider.Object);

        var mockVectorStore = new Mock<IVectorStore>();
        mockVectorStore.Setup(x => x.QueryAsync(It.IsAny<double[]>(), It.IsAny<int>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>())) // Changed to Dictionary<string, string>
            .ReturnsAsync(new List<VectorStoreQueryResult>
            {
                new VectorStoreQueryResult { Content = "Relevant context 1", Score = 0.8f },
                new VectorStoreQueryResult { Content = "Relevant context 2", Score = 0.75f }
            });
        _mockVectorStoreFactory.Setup(x => x.CreateVectorStore(It.IsAny<VectorStoreProviderType>())).Returns(mockVectorStore.Object);

        var mockChatProvider = new Mock<IChatProvider>();
        mockChatProvider.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())) // Removed CancellationToken
            .ReturnsAsync("AI response from context");
        _mockChatProviderFactory.Setup(x => x.GetProvider(It.IsAny<ChatAIProvider>())).Returns(mockChatProvider.Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Response.Should().Be("AI response from context");
        result.Value!.SessionId.Should().Be(sessionId);
        result.Value!.Model.Should().Be("Gemini");
        result.Value!.Response.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFallbackResponse_WhenNoContextFound()
    {
        // Arrange
        var userMessage = "Test message with no context";
        var sessionId = "test-session-no-context";
        var query = new ChatWithAssistantQuery(userMessage, sessionId);

        var embeddingSettings = new EmbeddingSettings { Provider = "OpenAI" };
        var vectorStoreSettings = new VectorStoreSettings { Provider = "Pinecone", TopK = 3 };
        var chatSettings = new AIChatSettings { Provider = "Gemini", ScoreThreshold = 70 };

        _mockConfigProvider.Setup(x => x.GetSection<EmbeddingSettings>()).Returns(embeddingSettings);
        _mockConfigProvider.Setup(x => x.GetSection<VectorStoreSettings>()).Returns(vectorStoreSettings);
        _mockConfigProvider.Setup(x => x.GetSection<AIChatSettings>()).Returns(chatSettings);

        var mockEmbeddingProvider = new Mock<IEmbeddingProvider>();
        mockEmbeddingProvider.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<double[]>.Success(new double[] { 0.1, 0.2, 0.3 })); // Changed to double[]
        _mockEmbeddingProviderFactory.Setup(x => x.GetProvider(It.IsAny<EmbeddingAIProvider>())).Returns(mockEmbeddingProvider.Object);

        var mockVectorStore = new Mock<IVectorStore>();
        mockVectorStore.Setup(x => x.QueryAsync(It.IsAny<double[]>(), It.IsAny<int>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>())) // Changed to Dictionary<string, string>
            .ReturnsAsync(new List<VectorStoreQueryResult>()); // No relevant context
        _mockVectorStoreFactory.Setup(x => x.CreateVectorStore(It.IsAny<VectorStoreProviderType>())).Returns(mockVectorStore.Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Response.Should().Be("Tôi không tìm thấy thông tin liên quan trong cơ sở dữ liệu. Bạn có câu hỏi nào khác về phần mềm không?");
        result.Value!.SessionId.Should().Be(sessionId);
        result.Value!.Model.Should().Be("Fallback");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmbeddingGenerationFails()
    {
        // Arrange
        var userMessage = "Test message";
        var query = new ChatWithAssistantQuery(userMessage);

        var embeddingSettings = new EmbeddingSettings { Provider = "OpenAI" };
        _mockConfigProvider.Setup(x => x.GetSection<EmbeddingSettings>()).Returns(embeddingSettings);

        var mockEmbeddingProvider = new Mock<IEmbeddingProvider>();
        mockEmbeddingProvider.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<double[]>.Failure("Embedding error")); // Changed to double[]
        _mockEmbeddingProviderFactory.Setup(x => x.GetProvider(It.IsAny<EmbeddingAIProvider>())).Returns(mockEmbeddingProvider.Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Failed to generate embedding: Embedding error");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenChatResponseGenerationFails()
    {
        // Arrange
        var userMessage = "Test message";
        var sessionId = "test-session";
        var query = new ChatWithAssistantQuery(userMessage, sessionId);

        var embeddingSettings = new EmbeddingSettings { Provider = "OpenAI" };
        var vectorStoreSettings = new VectorStoreSettings { Provider = "Pinecone", TopK = 3 };
        var chatSettings = new AIChatSettings { Provider = "Gemini", ScoreThreshold = 70 };

        _mockConfigProvider.Setup(x => x.GetSection<EmbeddingSettings>()).Returns(embeddingSettings);
        _mockConfigProvider.Setup(x => x.GetSection<VectorStoreSettings>()).Returns(vectorStoreSettings);
        _mockConfigProvider.Setup(x => x.GetSection<AIChatSettings>()).Returns(chatSettings);

        var mockEmbeddingProvider = new Mock<IEmbeddingProvider>();
        mockEmbeddingProvider.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<double[]>.Success(new double[] { 0.1, 0.2, 0.3 })); // Changed to double[]
        _mockEmbeddingProviderFactory.Setup(x => x.GetProvider(It.IsAny<EmbeddingAIProvider>())).Returns(mockEmbeddingProvider.Object);

        var mockVectorStore = new Mock<IVectorStore>();
        mockVectorStore.Setup(x => x.QueryAsync(It.IsAny<double[]>(), It.IsAny<int>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>())) // Changed to Dictionary<string, string>
            .ReturnsAsync(new List<VectorStoreQueryResult>
            {
                new VectorStoreQueryResult { Content = "Relevant context 1", Score = 0.8f }
            });
        _mockVectorStoreFactory.Setup(x => x.CreateVectorStore(It.IsAny<VectorStoreProviderType>())).Returns(mockVectorStore.Object);

        var mockChatProvider = new Mock<IChatProvider>();
        mockChatProvider.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())) // Removed CancellationToken
            .ThrowsAsync(new Exception("Chat generation error")); // Simulate chat provider failure
        _mockChatProviderFactory.Setup(x => x.GetProvider(It.IsAny<ChatAIProvider>())).Returns(mockChatProvider.Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Failed to generate chat response: Chat generation error");
    }
}
