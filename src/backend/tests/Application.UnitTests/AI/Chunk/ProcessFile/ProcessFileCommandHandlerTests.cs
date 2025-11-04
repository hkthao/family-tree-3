using System.Text;
using backend.Application.AI.Chunk.ProcessFile;
using backend.Application.Common.Interfaces;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.AI.Chunk.ProcessFile;

/// <summary>
/// 🎯 Mục tiêu: Kiểm thử hành vi của ProcessFileCommandHandler.
/// ⚙️ Các bước: Arrange - Act - Assert.
/// 💡 Giải thích: Đảm bảo handler xử lý đúng các trường hợp trích xuất và chia chunk tệp.
/// </summary>
public class ProcessFileCommandHandlerTests : TestBase
{
    private readonly Mock<IFileTextExtractorFactory> _mockExtractorFactory;
    private readonly Mock<IChunkingPolicy> _mockChunkingPolicy;

    public ProcessFileCommandHandlerTests()
    {
        _mockExtractorFactory = new Mock<IFileTextExtractorFactory>();
        _mockChunkingPolicy = new Mock<IChunkingPolicy>();


    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra handler trả về thất bại khi không tìm thấy trình trích xuất cho phần mở rộng tệp.
    /// ⚙️ Arrange: Cấu hình _mockExtractorFactory để GetExtractor ném ra ArgumentException.
    /// ⚙️ Act: Tạo ProcessFileCommand và gọi Handle của handler.
    /// ⚙️ Assert: Kỳ vọng Result.Failure với thông báo lỗi ArgumentException.
    /// 💡 Giải thích: Handler phải xử lý trường hợp tệp không được hỗ trợ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNoExtractorFoundForFileExtension()
    {
        // Arrange
        var command = new ProcessFileCommand { FileName = "test.unsupported", FileId = Guid.NewGuid().ToString(), FamilyId = Guid.NewGuid().ToString(), Category = "category", CreatedBy = "createdBy", FileStream = new MemoryStream() };
        _mockExtractorFactory.Setup(x => x.GetExtractor(It.IsAny<string>()))
            .Throws(new ArgumentException("No extractor found for .unsupported"));

        // Act
        var _handler = new ProcessFileCommandHandler(
            _mockExtractorFactory.Object,
            _mockChunkingPolicy.Object);
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No extractor found for .unsupported");
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra handler trả về thành công với danh sách rỗng khi tệp trống.
    /// ⚙️ Arrange: Cấu hình mock extractor để trả về chuỗi trống và mock chunking policy để trả về danh sách chunk rỗng.
    /// ⚙️ Act: Tạo ProcessFileCommand và gọi Handle của handler.
    /// ⚙️ Assert: Kỳ vọng Result.Success và danh sách chunks rỗng.
    /// 💡 Giải thích: Handler phải xử lý đúng trường hợp tệp không có nội dung.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnSuccessWithEmptyList_WhenFileIsEmpty()
    {
        // Arrange
        var mockExtractor = new Mock<IFileTextExtractor>();
        mockExtractor.Setup(x => x.ExtractTextAsync(It.IsAny<Stream>())).ReturnsAsync(string.Empty);
        _mockExtractorFactory.Setup(x => x.GetExtractor(It.IsAny<string>())).Returns(mockExtractor.Object);

        _mockChunkingPolicy.Setup(x => x.ChunkText(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new List<TextChunk>());
        var command = new ProcessFileCommand { FileName = "test.txt", FileId = Guid.NewGuid().ToString(), FamilyId = Guid.NewGuid().ToString(), Category = "category", CreatedBy = "createdBy", FileStream = new MemoryStream() };

        // Act
        var _handler = new ProcessFileCommandHandler(
          _mockExtractorFactory.Object,
          _mockChunkingPolicy.Object);
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        mockExtractor.Verify(x => x.ExtractTextAsync(It.IsAny<Stream>()), Times.Once);
        _mockChunkingPolicy.Verify(x => x.ChunkText(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra handler trả về thành công với các chunks khi tệp được xử lý thành công.
    /// ⚙️ Arrange: Cấu hình mock extractor để trả về nội dung văn bản và mock chunking policy để trả về danh sách chunks.
    /// ⚙️ Act: Tạo ProcessFileCommand và gọi Handle của handler.
    /// ⚙️ Assert: Kỳ vọng Result.Success và danh sách chunks không rỗng, khớp với dữ liệu mock.
    /// 💡 Giải thích: Đây là trường hợp thành công chính của handler.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnSuccessWithChunks_WhenFileIsProcessedSuccessfully()
    {
        // Arrange
        var fileContent = "This is a test file content.";
        var expectedChunks = new List<TextChunk>
        {
            new TextChunk { Id = "chunkId1", Content = "This is a", Source = "test.txt", FamilyId = Guid.NewGuid(), Category = "category"},
            new TextChunk { Id = "chunkId2", Content = "test file content.", Source = "test.txt", FamilyId = Guid.NewGuid(), Category = "category" }
        };

        var mockExtractor = new Mock<IFileTextExtractor>();
        mockExtractor.Setup(x => x.ExtractTextAsync(It.IsAny<Stream>())).ReturnsAsync(fileContent);
        _mockExtractorFactory.Setup(x => x.GetExtractor(It.IsAny<string>())).Returns(mockExtractor.Object);

        _mockChunkingPolicy.Setup(x => x.ChunkText(fileContent, "test.txt", It.IsAny<string>(), It.IsAny<string>(), "category", "createdBy"))
            .Returns(expectedChunks);

        byte[] byteArray = Encoding.UTF8.GetBytes(fileContent);
        var command = new ProcessFileCommand()
        {
            FileName = "test.txt",
            Category = "category",
            CreatedBy = "createdBy",
            FileStream = new MemoryStream(byteArray)
        };

        // Act
        var _handler = new ProcessFileCommandHandler(
          _mockExtractorFactory.Object,
          _mockChunkingPolicy.Object);
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedChunks);
        mockExtractor.Verify(x => x.ExtractTextAsync(It.IsAny<Stream>()), Times.Once);
        _mockChunkingPolicy.Verify(x => x.ChunkText(fileContent, "test.txt", It.IsAny<string>(), It.IsAny<string>(), "category", "createdBy"), Times.Once);
    }
}
