using backend.Application.AI.Chunk.EmbedChunks;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.AI.Chunk.EmbedChunks;

/// <summary>
/// 🎯 Mục tiêu: Kiểm thử hành vi của EmbedChunksCommandHandler.
/// ⚙️ Các bước: Arrange - Act - Assert.
/// 💡 Giải thích: Đảm bảo handler xử lý đúng các trường hợp thành công, thất bại và lỗi khi nhúng và lưu trữ các đoạn văn bản.
/// </summary>
public class EmbedChunksCommandHandlerTests : TestBase
{
    private readonly Mock<IEmbeddingProviderFactory> _mockEmbeddingProviderFactory;
    private readonly Mock<IVectorStoreFactory> _mockVectorStoreFactory;
    private readonly Mock<IConfigProvider> _mockConfigProvider;

    public EmbedChunksCommandHandlerTests()
    {
        _mockEmbeddingProviderFactory = new Mock<IEmbeddingProviderFactory>();
        _mockVectorStoreFactory = new Mock<IVectorStoreFactory>();
        _mockConfigProvider = new Mock<IConfigProvider>();
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra handler trả về thất bại khi danh sách chunks là null hoặc rỗng.
    /// ⚙️ Arrange: Tạo EmbedChunksCommand với danh sách chunks là null hoặc rỗng.
    /// ⚙️ Act: Gọi phương thức Handle của handler.
    /// ⚙️ Assert: Kỳ vọng Result.Failure với thông báo lỗi NotFound.
    /// 💡 Giải thích: Handler phải xử lý trường hợp không có chunk nào để nhúng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenChunksAreNullOrEmpty()
    {
        // Arrange
        var command = new EmbedChunksCommand { };

        // Act
        var _handler = new EmbedChunksCommandHandler(
            _mockEmbeddingProviderFactory.Object,
            _mockVectorStoreFactory.Object,
            _mockConfigProvider.Object);
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, "Chunks"));
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra handler trả về thất bại khi tên EmbeddingProvider không hợp lệ.
    /// ⚙️ Arrange: Cấu hình _mockConfigProvider để trả về EmbeddingSettings với Provider không hợp lệ.
    /// ⚙️ Act: Gọi phương thức Handle của handler.
    /// ⚙️ Assert: Kỳ vọng Result.Failure với thông báo lỗi ArgumentException.
    /// 💡 Giải thích: Handler phải xử lý lỗi khi không thể tạo EmbeddingProvider.
    /// </summary>
    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra handler trả về thất bại khi tên EmbeddingProvider không hợp lệ.
    /// ⚙️ Arrange: Cấu hình _mockConfigProvider để trả về EmbeddingSettings với Provider không hợp lệ.
    /// ⚙️ Act: Gọi phương thức Handle của handler.
    /// ⚙️ Assert: Kỳ vọng Result.Failure với thông báo lỗi ArgumentException.
    /// 💡 Giải thích: Handler phải xử lý lỗi khi không thể tạo EmbeddingProvider.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmbeddingProviderIsInvalid()
    {
        // Arrange
        var command = new EmbedChunksCommand { Chunks = new List<TextChunk> { new TextChunk { Id = "id1", Content = "content1", FamilyId = Guid.NewGuid(), Category = "category" } } };
        var embeddingSettings = new EmbeddingSettings { Provider = "InvalidProvider" };
        _mockConfigProvider.Setup(x => x.GetSection<EmbeddingSettings>()).Returns(embeddingSettings);

        // Act
        var _handler = new EmbedChunksCommandHandler(
          _mockEmbeddingProviderFactory.Object,
          _mockVectorStoreFactory.Object,
          _mockConfigProvider.Object);
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Requested value 'InvalidProvider' was not found.");
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra handler trả về thất bại khi quá trình tạo embedding thất bại.
    /// ⚙️ Arrange: Cấu hình _mockEmbeddingProviderFactory để trả về một EmbeddingProvider mà GenerateEmbeddingAsync trả về Result.Failure.
    /// ⚙️ Act: Gọi phương thức Handle của handler.
    /// ⚙️ Assert: Kỳ vọng Result.Failure với thông báo lỗi cụ thể.
    /// 💡 Giải thích: Handler phải xử lý lỗi khi không thể tạo embedding cho một chunk.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmbeddingGenerationFails()
    {
        // Arrange
        var command = new EmbedChunksCommand() { Chunks = new List<TextChunk> { new() { Id = "id1", Content = "content1", FamilyId = Guid.NewGuid(), Category = "category" } } };
        var embeddingSettings = new EmbeddingSettings { Provider = "OpenAI" };
        var mockVectorStore = new Mock<IVectorStore>();
        var mockEmbeddingProvider = new Mock<IEmbeddingProvider>();
        var vectorSettings = new VectorStoreSettings { Provider = "Pinecone" };

        _mockConfigProvider.Setup(x => x.GetSection<VectorStoreSettings>()).Returns(vectorSettings);
        _mockConfigProvider.Setup(x => x.GetSection<EmbeddingSettings>()).Returns(embeddingSettings);
        mockEmbeddingProvider.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<double[]>.Failure("Embedding generation error"));
        _mockEmbeddingProviderFactory.Setup(x => x.GetProvider(It.IsAny<EmbeddingAIProvider>())).Returns(mockEmbeddingProvider.Object);
        _mockVectorStoreFactory.Setup(x => x.CreateVectorStore(It.IsAny<VectorStoreProviderType>())).Returns(mockVectorStore.Object);

        // Act
        var _handler = new EmbedChunksCommandHandler(
          _mockEmbeddingProviderFactory.Object,
          _mockVectorStoreFactory.Object,
          _mockConfigProvider.Object);
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Failed to generate embedding for chunk id1: Embedding generation error");
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra handler trả về thất bại khi embedding được tạo ra là null hoặc rỗng.
    /// ⚙️ Arrange: Cấu hình _mockEmbeddingProviderFactory để trả về một EmbeddingProvider mà GenerateEmbeddingAsync trả về embedding null hoặc rỗng.
    /// ⚙️ Act: Gọi phương thức Handle của handler.
    /// ⚙️ Assert: Kỳ vọng Result.Failure với thông báo lỗi cụ thể.
    /// 💡 Giải thích: Handler phải đảm bảo embedding được tạo ra là hợp lệ.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData(new double[0])]
    public async Task Handle_ShouldReturnFailure_WhenGeneratedEmbeddingIsNullOrEmpty(double[]? embeddingValue)
    {
        // Arrange
        var command = new EmbedChunksCommand { Chunks = new List<TextChunk> { new TextChunk { Id = "id1", Content = "content1", FamilyId = Guid.NewGuid(), Category = "category" } } };
        var embeddingSettings = new EmbeddingSettings { Provider = "OpenAI" };
        var vectorSettings = new VectorStoreSettings { Provider = "Pinecone" };
        var mockEmbeddingProvider = new Mock<IEmbeddingProvider>();
        var mockVectorStore = new Mock<IVectorStore>();

        _mockConfigProvider.Setup(x => x.GetSection<EmbeddingSettings>()).Returns(embeddingSettings);
        _mockConfigProvider.Setup(x => x.GetSection<VectorStoreSettings>()).Returns(vectorSettings);
        mockEmbeddingProvider.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<double[]>.Success(embeddingValue!));
        _mockEmbeddingProviderFactory.Setup(x => x.GetProvider(It.IsAny<EmbeddingAIProvider>())).Returns(mockEmbeddingProvider.Object);
        _mockVectorStoreFactory.Setup(x => x.CreateVectorStore(It.IsAny<VectorStoreProviderType>())).Returns(mockVectorStore.Object);

        // Act
        var _handler = new EmbedChunksCommandHandler(
          _mockEmbeddingProviderFactory.Object,
          _mockVectorStoreFactory.Object,
          _mockConfigProvider.Object);
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Generated embedding for chunk id1 is null or empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra handler trả về thành công khi tất cả các chunks được nhúng và lưu trữ thành công.
    /// ⚙️ Arrange: Cấu hình tất cả các mock để mô phỏng quá trình nhúng và lưu trữ thành công.
    /// ⚙️ Act: Gọi phương thức Handle của handler.
    /// ⚙️ Assert: Kỳ vọng Result.Success. Xác minh rằng GenerateEmbeddingAsync và UpsertAsync được gọi đúng số lần.
    /// 💡 Giải thích: Đây là trường hợp thành công chính của handler.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAllChunksAreEmbeddedSuccessfully()
    {
        // Arrange
        var chunks = new List<TextChunk>
        {
            new TextChunk { Id = "id1", Content = "content1", FamilyId = Guid.NewGuid(), Category = "category" },
            new TextChunk { Id = "id2", Content = "content2", FamilyId = Guid.NewGuid(), Category = "category" }
        };
        var command = new EmbedChunksCommand(chunks);

        var embeddingSettings = new EmbeddingSettings { Provider = "OpenAI" };
        var vectorStoreSettings = new VectorStoreSettings { Provider = "Pinecone", TopK = 3 };
        _mockConfigProvider.Setup(x => x.GetSection<EmbeddingSettings>()).Returns(embeddingSettings);
        _mockConfigProvider.Setup(x => x.GetSection<VectorStoreSettings>()).Returns(vectorStoreSettings);

        var mockEmbeddingProvider = new Mock<IEmbeddingProvider>();
        mockEmbeddingProvider.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<double[]>.Success(new double[] { 0.1, 0.2, 0.3 }));
        _mockEmbeddingProviderFactory.Setup(x => x.GetProvider(It.IsAny<EmbeddingAIProvider>())).Returns(mockEmbeddingProvider.Object);

        var mockVectorStore = new Mock<IVectorStore>();
        _mockVectorStoreFactory.Setup(x => x.CreateVectorStore(It.IsAny<VectorStoreProviderType>())).Returns(mockVectorStore.Object);
        mockVectorStore.Setup(x => x.UpsertAsync(It.IsAny<List<double>>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var _handler = new EmbedChunksCommandHandler(
          _mockEmbeddingProviderFactory.Object,
          _mockVectorStoreFactory.Object,
          _mockConfigProvider.Object);
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        mockEmbeddingProvider.Verify(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(chunks.Count));
        mockVectorStore.Verify(x => x.UpsertAsync(It.IsAny<List<double>>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Exactly(chunks.Count));
    }
}
