using backend.Application.AI.Chunk.EmbedChunks;
using backend.Application.Common.Models.AppSetting;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Xunit;
using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Moq;

namespace backend.Application.UnitTests.AI.Chunk.EmbedChunks;

/// <summary>
/// Lớp kiểm thử đơn vị cho EmbedChunksCommandHandler.
/// Đảm bảo rằng EmbedChunksCommandHandler xử lý đúng các lệnh nhúng (embedding) các đoạn văn bản.
/// </summary>
public class EmbedChunksCommandHandlerTests : AITestBase<EmbedChunksCommandHandler>
{
    private readonly EmbedChunksCommandHandler _handler; // Đối tượng handler cần kiểm thử

    /// <summary>
    /// Constructor để khởi tạo các mock và đối tượng handler trước mỗi bài kiểm thử.
    /// </summary>
    public EmbedChunksCommandHandlerTests()
    {
        // Khởi tạo handler với các mock dependency
        _handler = new EmbedChunksCommandHandler(
            _mockEmbeddingProviderFactory.Object,
            _mockVectorStoreFactory.Object,
            _mockConfigProvider.Object);
    }

    /// <summary>
    /// Kiểm thử kịch bản thành công: khi các đoạn văn bản được nhúng và lưu trữ thành công.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenChunksAreEmbeddedAndUpsertedSuccessfully()
    {
        // Arrange (Thiết lập): Chuẩn bị dữ liệu và hành vi của các mock
        var chunks = _fixture.CreateMany<TextChunk>(3).ToList(); // Tạo 3 đoạn văn bản giả
        var command = new EmbedChunksCommand { Chunks = chunks }; // Tạo command với các đoạn văn bản

        // Thiết lập cài đặt Embedding
        var embeddingSettings = _fixture.Create<EmbeddingSettings>();
        embeddingSettings.Provider = EmbeddingAIProvider.OpenAI.ToString();
        _mockConfigProvider.Setup(x => x.GetSection<EmbeddingSettings>()).Returns(embeddingSettings);

        // Mock IEmbeddingProvider để trả về embedding thành công cho mỗi đoạn
        var embeddingProviderMock = new Mock<IEmbeddingProvider>();
        embeddingProviderMock.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<double[]>.Success(_fixture.Create<double[]>())); // Trả về embedding dạng double[]
        _mockEmbeddingProviderFactory.Setup(x => x.GetProvider(It.IsAny<EmbeddingAIProvider>())).Returns(embeddingProviderMock.Object);

        // Thiết lập cài đặt VectorStore
        var vectorStoreSettings = _fixture.Create<VectorStoreSettings>();
        vectorStoreSettings.Provider = VectorStoreProviderType.Pinecone.ToString();
        _mockConfigProvider.Setup(x => x.GetSection<VectorStoreSettings>()).Returns(vectorStoreSettings);

        // Mock IVectorStore để xác nhận UpsertAsync được gọi
        var vectorStoreMock = new Mock<IVectorStore>();
        vectorStoreMock.Setup(x => x.UpsertAsync(It.IsAny<List<double>>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockVectorStoreFactory.Setup(x => x.CreateVectorStore(It.IsAny<VectorStoreProviderType>())).Returns(vectorStoreMock.Object);

        // Act (Thực thi): Gọi phương thức Handle của handler
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra): Xác minh kết quả
        Assert.True(result.IsSuccess); // Đảm bảo kết quả là thành công
        // Xác minh GenerateEmbeddingAsync được gọi số lần bằng số lượng đoạn văn bản
        embeddingProviderMock.Verify(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(chunks.Count));
        // Xác minh UpsertAsync được gọi số lần bằng số lượng đoạn văn bản
        vectorStoreMock.Verify(x => x.UpsertAsync(It.IsAny<List<double>>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Exactly(chunks.Count));
    }

    /// <summary>
    /// Kiểm thử kịch bản thất bại: khi không có đoạn văn bản nào được cung cấp.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNoChunksProvided()
    {
        // Arrange (Thiết lập)
        var command = new EmbedChunksCommand { Chunks = new List<TextChunk>() }; // Command với danh sách đoạn văn bản rỗng

        // Act (Thực thi)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra)
        Assert.False(result.IsSuccess); // Đảm bảo kết quả là thất bại
        Assert.Equal("No chunks provided for embedding.", result.Error); // Kiểm tra thông báo lỗi
    }

    /// <summary>
    /// Kiểm thử kịch bản thất bại: khi không tìm thấy hoặc provider nhúng không hợp lệ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmbeddingProviderNotFound()
    {
        // Arrange (Thiết lập)
        var chunks = _fixture.CreateMany<TextChunk>(1).ToList();
        var command = new EmbedChunksCommand { Chunks = chunks };

        // Thiết lập cài đặt Embedding với provider không hợp lệ
        var embeddingSettings = _fixture.Create<EmbeddingSettings>();
        embeddingSettings.Provider = "InvalidProvider"; // Mô phỏng provider không hợp lệ
        _mockConfigProvider.Setup(x => x.GetSection<EmbeddingSettings>()).Returns(embeddingSettings);

        // Mock IEmbeddingProviderFactory để ném ra ArgumentException khi GetProvider được gọi
        _mockEmbeddingProviderFactory.Setup(x => x.GetProvider(It.IsAny<EmbeddingAIProvider>()))
            .Throws(new ArgumentException("Invalid embedding provider."));

        // Act (Thực thi)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra)
        Assert.False(result.IsSuccess); // Đảm bảo kết quả là thất bại
        Assert.Contains("InvalidProvider", result.Error); // Kiểm tra thông báo lỗi chứa 'InvalidProvider' để linh hoạt hơn với thông báo lỗi từ Enum.Parse
    }

    /// <summary>
    /// Kiểm thử kịch bản thất bại: khi quá trình tạo embedding cho một đoạn văn bản gặp lỗi.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmbeddingGenerationFails()
    {
        // Arrange (Thiết lập)
        var chunks = _fixture.CreateMany<TextChunk>(1).ToList();
        var command = new EmbedChunksCommand { Chunks = chunks };

        // Thiết lập cài đặt Embedding
        var embeddingSettings = _fixture.Create<EmbeddingSettings>();
        embeddingSettings.Provider = EmbeddingAIProvider.OpenAI.ToString();
        _mockConfigProvider.Setup(x => x.GetSection<EmbeddingSettings>()).Returns(embeddingSettings);

        // Mock IEmbeddingProvider để trả về lỗi khi tạo embedding
        var embeddingProviderMock = new Mock<IEmbeddingProvider>();
        embeddingProviderMock.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<double[]>.Failure("Embedding generation failed."));
        _mockEmbeddingProviderFactory.Setup(x => x.GetProvider(It.IsAny<EmbeddingAIProvider>())).Returns(embeddingProviderMock.Object);

        // Thiết lập cài đặt VectorStore
        var vectorStoreSettings = _fixture.Create<VectorStoreSettings>();
        vectorStoreSettings.Provider = VectorStoreProviderType.Pinecone.ToString();
        _mockConfigProvider.Setup(x => x.GetSection<VectorStoreSettings>()).Returns(vectorStoreSettings);

        // Mock IVectorStore (không cần thiết lập hành vi UpsertAsync vì nó sẽ không được gọi)
        var vectorStoreMock = new Mock<IVectorStore>();
        _mockVectorStoreFactory.Setup(x => x.CreateVectorStore(It.IsAny<VectorStoreProviderType>())).Returns(vectorStoreMock.Object);

        // Act (Thực thi)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra)
        Assert.False(result.IsSuccess); // Đảm bảo kết quả là thất bại
        Assert.Equal($"Failed to generate embedding for chunk {chunks[0].Id}: Embedding generation failed.", result.Error); // Kiểm tra thông báo lỗi
        // Xác minh UpsertAsync không bao giờ được gọi
        vectorStoreMock.Verify(x => x.UpsertAsync(It.IsAny<List<double>>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Kiểm thử kịch bản thất bại: khi embedding được tạo ra là null.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenGeneratedEmbeddingIsNull()
    {
        // Arrange (Thiết lập)
        var chunks = _fixture.CreateMany<TextChunk>(1).ToList();
        var command = new EmbedChunksCommand { Chunks = chunks };

        // Thiết lập cài đặt Embedding
        var embeddingSettings = _fixture.Create<EmbeddingSettings>();
        embeddingSettings.Provider = EmbeddingAIProvider.OpenAI.ToString();
        _mockConfigProvider.Setup(x => x.GetSection<EmbeddingSettings>()).Returns(embeddingSettings);

        // Mock IEmbeddingProvider để trả về embedding là null
        var embeddingProviderMock = new Mock<IEmbeddingProvider>();
        embeddingProviderMock.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<double[]>.Success((double[])null!)); // Mô phỏng embedding null
        _mockEmbeddingProviderFactory.Setup(x => x.GetProvider(It.IsAny<EmbeddingAIProvider>())).Returns(embeddingProviderMock.Object);

        // Thiết lập cài đặt VectorStore
        var vectorStoreSettings = _fixture.Create<VectorStoreSettings>();
        vectorStoreSettings.Provider = VectorStoreProviderType.Pinecone.ToString();
        _mockConfigProvider.Setup(x => x.GetSection<VectorStoreSettings>()).Returns(vectorStoreSettings);

        // Mock IVectorStore (không cần thiết lập hành vi UpsertAsync vì nó sẽ không được gọi)
        var vectorStoreMock = new Mock<IVectorStore>();
        _mockVectorStoreFactory.Setup(x => x.CreateVectorStore(It.IsAny<VectorStoreProviderType>())).Returns(vectorStoreMock.Object);

        // Act (Thực thi)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra)
        Assert.False(result.IsSuccess); // Đảm bảo kết quả là thất bại
        Assert.Equal($"Generated embedding for chunk {chunks[0].Id} is null or empty.", result.Error); // Kiểm tra thông báo lỗi
        // Xác minh UpsertAsync không bao giờ được gọi
        vectorStoreMock.Verify(x => x.UpsertAsync(It.IsAny<List<double>>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Kiểm thử kịch bản thất bại: khi embedding được tạo ra là rỗng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenGeneratedEmbeddingIsEmpty()
    {
        // Arrange (Thiết lập)
        var chunks = _fixture.CreateMany<TextChunk>(1).ToList();
        var command = new EmbedChunksCommand { Chunks = chunks };

        // Thiết lập cài đặt Embedding
        var embeddingSettings = _fixture.Create<EmbeddingSettings>();
        embeddingSettings.Provider = EmbeddingAIProvider.OpenAI.ToString();
        _mockConfigProvider.Setup(x => x.GetSection<EmbeddingSettings>()).Returns(embeddingSettings);

        // Mock IEmbeddingProvider để trả về embedding là rỗng
        var embeddingProviderMock = new Mock<IEmbeddingProvider>();
        embeddingProviderMock.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<double[]>.Success(Array.Empty<double>())); // Mô phỏng embedding rỗng
        _mockEmbeddingProviderFactory.Setup(x => x.GetProvider(It.IsAny<EmbeddingAIProvider>())).Returns(embeddingProviderMock.Object);

        // Thiết lập cài đặt VectorStore
        var vectorStoreSettings = _fixture.Create<VectorStoreSettings>();
        vectorStoreSettings.Provider = VectorStoreProviderType.Pinecone.ToString();
        _mockConfigProvider.Setup(x => x.GetSection<VectorStoreSettings>()).Returns(vectorStoreSettings);

        // Mock IVectorStore (không cần thiết lập hành vi UpsertAsync vì nó sẽ không được gọi)
        var vectorStoreMock = new Mock<IVectorStore>();
        _mockVectorStoreFactory.Setup(x => x.CreateVectorStore(It.IsAny<VectorStoreProviderType>())).Returns(vectorStoreMock.Object);

        // Act (Thực thi)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra)
        Assert.False(result.IsSuccess); // Đảm bảo kết quả là thất bại
        Assert.Equal($"Generated embedding for chunk {chunks[0].Id} is null or empty.", result.Error); // Kiểm tra thông báo lỗi
        // Xác minh UpsertAsync không bao giờ được gọi
        vectorStoreMock.Verify(x => x.UpsertAsync(It.IsAny<List<double>>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
