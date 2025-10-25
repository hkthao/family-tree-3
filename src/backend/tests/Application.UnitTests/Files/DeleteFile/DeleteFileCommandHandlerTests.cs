using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.DeleteFile;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Files.DeleteFile;

public class DeleteFileCommandHandlerTests : TestBase
{
    private readonly Mock<IFileStorage> _mockFileStorage;
    private readonly DeleteFileCommandHandler _handler;

    public DeleteFileCommandHandlerTests()
    {
        _mockFileStorage = new Mock<IFileStorage>();
        _handler = new DeleteFileCommandHandler(
            _context,
            _mockFileStorage.Object,
            _mockUser.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileMetadataNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy siêu dữ liệu tệp.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Đảm bảo không có FileMetadata nào trong Context với FileId được yêu cầu.
        // 2. Act: Gọi phương thức Handle với một DeleteFileCommand bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var command = new DeleteFileCommand { FileId = Guid.NewGuid() };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("File metadata not found.");
        result.ErrorSource.Should().Be("NotFound");
        // 💡 Giải thích: Không thể xóa tệp nếu không tìm thấy siêu dữ liệu của nó.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotAuthorized()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi người dùng không được ủy quyền xóa tệp.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo FileMetadata với UploadedBy khác với _user.Id.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var fileId = Guid.NewGuid();
        var fileMetadata = new FileMetadata
        {
            Id = fileId,
            FileName = "test.jpg",
            Url = "http://example.com/test.jpg",
            UploadedBy = Guid.NewGuid().ToString(), // Different user
            ContentType = "image/jpeg", // Thêm ContentType
            IsActive = true
        };
        _context.FileMetadata.Add(fileMetadata);
        await _context.SaveChangesAsync();

        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid()); // Current user is different

        var command = new DeleteFileCommand { FileId = fileId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User is not authorized to delete this file.");
        result.ErrorSource.Should().Be("Forbidden");
        // 💡 Giải thích: Chỉ người tải lên mới có quyền xóa tệp.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileStorageDeletionFails()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi xóa tệp khỏi bộ lưu trữ thất bại.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo FileMetadata với UploadedBy khớp với _user.Id. Mock _fileStorage.DeleteFileAsync() trả về Result.Failure.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var fileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var fileMetadata = new FileMetadata
        {
            Id = fileId,
            FileName = "test.jpg",
            Url = "http://example.com/test.jpg",
            UploadedBy = userId.ToString(),
            ContentType = "image/jpeg", // Thêm ContentType
            IsActive = true
        };
        _context.FileMetadata.Add(fileMetadata);
        await _context.SaveChangesAsync();

        _mockUser.Setup(u => u.Id).Returns(userId);
        _mockFileStorage.Setup(fs => fs.DeleteFileAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Storage deletion failed.", "FileStorage"));

        var command = new DeleteFileCommand { FileId = fileId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Storage deletion failed.");
        result.ErrorSource.Should().Be("FileStorage");
        // 💡 Giải thích: Lỗi từ dịch vụ lưu trữ tệp phải được truyền lại.
    }

    [Fact]
    public async Task Handle_ShouldDeleteFileAndMetadataSuccessfully()
    {
        // 🎯 Mục tiêu của test: Xác minh handler xóa tệp và siêu dữ liệu thành công.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo FileMetadata với UploadedBy khớp với _user.Id. Mock _fileStorage.DeleteFileAsync() trả về Result.Success().
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công. Xác minh _fileStorage.DeleteFileAsync() được gọi.
        //             Xác minh FileMetadata bị xóa khỏi Context.
        var fileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var fileMetadata = new FileMetadata
        {
            Id = fileId,
            FileName = "test.jpg",
            Url = "http://example.com/test.jpg",
            UploadedBy = userId.ToString(),
            ContentType = "image/jpeg", // Thêm ContentType
            IsActive = true
        };
        _context.FileMetadata.Add(fileMetadata);
        await _context.SaveChangesAsync();

        _mockUser.Setup(u => u.Id).Returns(userId);
        _mockFileStorage.Setup(fs => fs.DeleteFileAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var command = new DeleteFileCommand { FileId = fileId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockFileStorage.Verify(fs => fs.DeleteFileAsync(fileMetadata.Url, It.IsAny<CancellationToken>()), Times.Once);
        _context.FileMetadata.Should().BeEmpty(); // Verify metadata is removed from DB
        // 💡 Giải thích: Tệp và siêu dữ liệu của nó phải được xóa thành công.
    }
}
