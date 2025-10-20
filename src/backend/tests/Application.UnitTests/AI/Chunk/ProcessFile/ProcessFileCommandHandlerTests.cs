using backend.Application.AI.Chunk.ProcessFile;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Services;
using Moq;
using Xunit;
using AutoFixture;
using System.Text;

namespace backend.Application.UnitTests.AI.Chunk.ProcessFile;

/// <summary>
/// Lớp kiểm thử đơn vị cho ProcessFileCommandHandler.
/// Đảm bảo rằng ProcessFileCommandHandler xử lý đúng các lệnh xử lý tệp, trích xuất văn bản và tạo các đoạn văn bản.
/// </summary>
public class ProcessFileCommandHandlerTests : AITestBase<ProcessFileCommandHandler>
{
    private readonly ProcessFileCommandHandler _handler; // Đối tượng handler cần kiểm thử
    private readonly Mock<IFileTextExtractorFactory> _mockFileTextExtractorFactory; // Mock cho factory trích xuất văn bản
    private readonly Mock<IFileTextExtractor> _mockFileTextExtractor; // Mock cho bộ trích xuất văn bản
    private readonly Mock<IChunkingPolicy> _mockChunkingPolicy; // Mock cho chính sách phân đoạn văn bản

    /// <summary>
    /// Constructor để khởi tạo các mock và đối tượng handler trước mỗi bài kiểm thử.
    /// </summary>
    public ProcessFileCommandHandlerTests()
    {
        _mockFileTextExtractorFactory = new Mock<IFileTextExtractorFactory>();
        _mockFileTextExtractor = new Mock<IFileTextExtractor>();
        _mockChunkingPolicy = new Mock<IChunkingPolicy>();

        // Khởi tạo handler với các mock dependency
        _handler = new ProcessFileCommandHandler(
            _mockFileTextExtractorFactory.Object,
            _mockChunkingPolicy.Object);
    }

    /// <summary>
    /// Kiểm thử kịch bản thành công: khi tệp được xử lý, văn bản được trích xuất và phân đoạn thành công.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenFileIsProcessedSuccessfully()
    {
        // Arrange (Thiết lập): Chuẩn bị dữ liệu và hành vi của các mock
        var fileName = "test.pdf";
        var fileContent = "This is some test content.";
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
        var expectedChunks = _fixture.CreateMany<TextChunk>(2).ToList(); // Tạo 2 đoạn văn bản giả

        var command = _fixture.Build<ProcessFileCommand>()
            .With(x => x.FileName, fileName)
            .With(x => x.FileStream, fileStream)
            .Create();

        // Thiết lập factory để trả về bộ trích xuất đã mock cho phần mở rộng .pdf
        _mockFileTextExtractorFactory.Setup(x => x.GetExtractor(".pdf"))
            .Returns(_mockFileTextExtractor.Object);

        // Thiết lập bộ trích xuất văn bản để trả về nội dung giả lập
        _mockFileTextExtractor.Setup(x => x.ExtractTextAsync(It.IsAny<Stream>()))
            .ReturnsAsync(fileContent);

        // Thiết lập chính sách phân đoạn văn bản để trả về các đoạn văn bản giả lập
        _mockChunkingPolicy.Setup(x => x.ChunkText(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(expectedChunks);

        // Act (Thực thi): Gọi phương thức Handle của handler
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra): Xác minh kết quả
        Assert.True(result.IsSuccess); // Đảm bảo kết quả là thành công
        Assert.NotNull(result.Value); // Đảm bảo giá trị không null
        Assert.Equal(expectedChunks.Count, result.Value.Count); // Đảm bảo số lượng đoạn văn bản khớp
        // Thiết lập mặc định cho factory trả về bộ trích xuất đã mock
        _mockFileTextExtractorFactory.Setup(x => x.GetExtractor(It.IsAny<string>()))
            .Returns(_mockFileTextExtractor.Object);
        _mockFileTextExtractor.Verify(x => x.ExtractTextAsync(fileStream), Times.Once); // Xác minh bộ trích xuất được gọi đúng
        _mockChunkingPolicy.Verify(x => x.ChunkText(
                fileContent,
                command.FileName,
                command.FileId,
                command.FamilyId,
                command.Category,
                command.CreatedBy), Times.Once); // Xác minh chính sách phân đoạn được gọi đúng
    }

    /// <summary>
    /// Kiểm thử kịch bản thất bại: khi phần mở rộng tệp không được hỗ trợ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUnsupportedFileExtension()
    {
        // Arrange (Thiết lập)
        var fileName = "test.xyz"; // Phần mở rộng không hợp lệ
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes("dummy content"));
        var errorMessage = "Unsupported file extension: .xyz";

        var command = _fixture.Build<ProcessFileCommand>()
            .With(x => x.FileName, fileName)
            .With(x => x.FileStream, fileStream)
            .Create();

        // Thiết lập factory để ném ra ArgumentException khi gặp phần mở rộng không hợp lệ
        _mockFileTextExtractorFactory.Setup(x => x.GetExtractor(".xyz"))
            .Throws(new ArgumentException(errorMessage));

        // Act (Thực thi)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra)
        Assert.False(result.IsSuccess); // Đảm bảo kết quả là thất bại
        Assert.Contains(errorMessage, result.Error); // Kiểm tra thông báo lỗi
        _mockFileTextExtractorFactory.Verify(x => x.GetExtractor(".xyz"), Times.Once); // Xác minh factory được gọi đúng
        _mockFileTextExtractor.Verify(x => x.ExtractTextAsync(It.IsAny<Stream>()), Times.Never); // Xác minh bộ trích xuất không được gọi
        _mockChunkingPolicy.Verify(x => x.ChunkText(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Never); // Xác minh chính sách phân đoạn không được gọi
    }

    /// <summary>
    /// Kiểm thử kịch bản thành công: khi nội dung tệp được trích xuất là rỗng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEmptyFileContent()
    {
        // Arrange (Thiết lập)
        var fileName = "test.txt";
        var fileContent = ""; // Nội dung tệp rỗng
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
        var expectedChunks = new List<TextChunk>(); // Danh sách đoạn văn bản rỗng

        var command = _fixture.Build<ProcessFileCommand>()
            .With(x => x.FileName, fileName)
            .With(x => x.FileStream, fileStream)
            .Create();

        // Thiết lập factory để trả về bộ trích xuất đã mock cho phần mở rộng .txt
        _mockFileTextExtractorFactory.Setup(x => x.GetExtractor(".txt"))
            .Returns(_mockFileTextExtractor.Object);

        // Thiết lập bộ trích xuất văn bản để trả về nội dung rỗng
        _mockFileTextExtractor.Setup(x => x.ExtractTextAsync(It.IsAny<Stream>()))
            .ReturnsAsync(fileContent);

        // Thiết lập chính sách phân đoạn văn bản để trả về danh sách rỗng
        _mockChunkingPolicy.Setup(x => x.ChunkText(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(expectedChunks);

        // Act (Thực thi)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra)
        Assert.True(result.IsSuccess); // Đảm bảo kết quả là thành công
        Assert.NotNull(result.Value); // Đảm bảo giá trị không null
        Assert.Empty(result.Value); // Đảm bảo danh sách đoạn văn bản là rỗng
        _mockFileTextExtractorFactory.Verify(x => x.GetExtractor(".txt"), Times.Once);
        _mockFileTextExtractor.Verify(x => x.ExtractTextAsync(fileStream), Times.Once);
        _mockChunkingPolicy.Verify(x => x.ChunkText(
                fileContent,
                command.FileName,
                command.FileId,
                command.FamilyId,
                command.Category,
                command.CreatedBy), Times.Once);
    }
}
