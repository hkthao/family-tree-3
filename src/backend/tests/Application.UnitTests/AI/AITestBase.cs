using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.UnitTests.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace backend.Application.UnitTests.AI;

/// <summary>
/// Lớp cơ sở cho các bài kiểm thử liên quan đến AI, kế thừa từ TestBase.
/// Cung cấp các mock chung cho các dependency liên quan đến AI để tránh lặp lại code.
/// </summary>
public abstract class AITestBase<THandler> : TestBase where THandler : class
{
    protected readonly Mock<IChatProviderFactory> _mockChatProviderFactory;
    protected readonly Mock<IEmbeddingProviderFactory> _mockEmbeddingProviderFactory;
    protected readonly Mock<IVectorStoreFactory> _mockVectorStoreFactory;
    protected readonly Mock<IConfigProvider> _mockConfigProvider;
    protected readonly Mock<ILogger<THandler>> _mockLogger;
    protected readonly Fixture _fixture;

    protected AITestBase()
    {
        _mockChatProviderFactory = new Mock<IChatProviderFactory>();
        _mockEmbeddingProviderFactory = new Mock<IEmbeddingProviderFactory>();
        _mockVectorStoreFactory = new Mock<IVectorStoreFactory>();
        _mockConfigProvider = new Mock<IConfigProvider>();
        _mockLogger = new Mock<ILogger<THandler>>();
        _fixture = new Fixture();
    }
}
