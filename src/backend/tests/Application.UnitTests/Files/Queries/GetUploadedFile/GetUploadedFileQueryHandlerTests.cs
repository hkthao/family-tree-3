using AutoFixture;
using AutoFixture.AutoMoq;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Files.Queries.GetUploadedFile;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.UnitTests.Common;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Application.UnitTests.Files.Queries.GetUploadedFile;

public class GetUploadedFileQueryHandlerTests : TestBase
{
    private readonly Mock<IConfigProvider> _mockConfigProvider;
    private readonly GetUploadedFileQueryHandler _handler;
    private readonly string _testStoragePath;

    public GetUploadedFileQueryHandlerTests()
    {
        _mockConfigProvider = new Mock<IConfigProvider>();
        _fixture.Customize(new AutoMoqCustomization());

        // Create a temporary directory for testing file storage
        _testStoragePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testStoragePath);

        _mockConfigProvider.Setup(cp => cp.GetSection<StorageSettings>())
            .Returns(new StorageSettings { Local = new LocalStorageSettings { LocalStoragePath = _testStoragePath } });

        _handler = new GetUploadedFileQueryHandler(
            _mockConfigProvider.Object
        );
    }

    public override void Dispose()
    {
        // Clean up the temporary directory after tests
        if (Directory.Exists(_testStoragePath))
        {
            Directory.Delete(_testStoragePath, true);
        }
        base.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenFileNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi tệp không tồn tại.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một GetUploadedFileQuery với tên tệp không tồn tại.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var query = new GetUploadedFileQuery { FileName = "nonexistent.txt" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("File not found.");
        result.ErrorSource.Should().Be("NotFound");
        // 💡 Giải thích: Handler phải báo lỗi khi tệp được yêu cầu không có trên hệ thống.
    }

    [Fact]
    public async Task Handle_ShouldReturnFileContentSuccessfully()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về nội dung tệp và kiểu nội dung chính xác khi tệp tồn tại.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một tệp vật lý trong thư mục lưu trữ tạm thời. Tạo một GetUploadedFileQuery với tên tệp đó.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công. Xác minh nội dung tệp và kiểu nội dung khớp với mong đợi.
        var fileName = "testfile.txt";
        var fileContent = "Hello, this is a test file.";
        var filePath = Path.Combine(_testStoragePath, fileName);
        await File.WriteAllTextAsync(filePath, fileContent);

        var query = new GetUploadedFileQuery { FileName = fileName };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Content.Should().Equal(System.Text.Encoding.UTF8.GetBytes(fileContent));
        result.Value.ContentType.Should().Be("application/octet-stream"); // Default for .txt
        // 💡 Giải thích: Handler phải đọc đúng nội dung tệp và xác định kiểu nội dung mặc định cho tệp .txt.
    }

    [Theory]
    [InlineData("image.jpg", "image/jpeg")]
    [InlineData("photo.jpeg", "image/jpeg")]
    [InlineData("document.png", "image/png")]
    [InlineData("report.pdf", "application/pdf")]
    [InlineData("letter.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("archive.zip", "application/octet-stream")] // Default for unknown types
    public async Task Handle_ShouldReturnCorrectContentTypeForDifferentExtensions(string fileName, string expectedContentType)
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về kiểu nội dung chính xác cho các phần mở rộng tệp khác nhau.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một tệp vật lý với phần mở rộng cụ thể trong thư mục lưu trữ tạm thời. Tạo một GetUploadedFileQuery với tên tệp đó.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và kiểu nội dung khớp với mong đợi.
        var filePath = Path.Combine(_testStoragePath, fileName);
        await File.WriteAllTextAsync(filePath, "dummy content");

        var query = new GetUploadedFileQuery { FileName = fileName };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ContentType.Should().Be(expectedContentType);
        // 💡 Giải thích: Handler phải xác định đúng kiểu nội dung dựa trên phần mở rộng tệp.
    }
}
