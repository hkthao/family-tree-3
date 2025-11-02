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
            _mockUser.Object,
            _mockDateTime.Object
        );
    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi không tìm thấy siêu dữ liệu tệp (FileMetadata) tương ứng với FileId được cung cấp.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Đảm bảo không có FileMetadata nào trong cơ sở dữ liệu với FileId được yêu cầu.
    ///    - Act: Gọi phương thức Handle của handler với một DeleteFileCommand bất kỳ.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thất bại (IsSuccess = false) và chứa thông báo lỗi phù hợp ("File metadata not found.").
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Hệ thống không thể xóa một tệp nếu không tìm thấy thông tin siêu dữ liệu của nó trong cơ sở dữ liệu, đảm bảo tính toàn vẹn dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileMetadataNotFound()
    {

        var command = new DeleteFileCommand { FileId = Guid.NewGuid() };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("File metadata not found.");
        result.ErrorSource.Should().Be("NotFound");

    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi người dùng hiện tại không được ủy quyền để xóa tệp (tức là không phải là người đã tải lên tệp).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một FileMetadata với UploadedBy là một người dùng khác với người dùng hiện tại (_mockUser.Id).
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thất bại (IsSuccess = false) và chứa thông báo lỗi phù hợp ("Access denied. You do not have permission to perform this action.").
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Chỉ người dùng đã tải lên tệp mới có quyền xóa tệp đó, đảm bảo quyền sở hữu và bảo mật.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotAuthorized()
    {

        var fileId = Guid.NewGuid();
        var fileMetadata = new FileMetadata
        {
            Id = fileId,
            FileName = "test.jpg",
            Url = "http://example.com/test.jpg",
            UploadedBy = Guid.NewGuid().ToString(), // Different user
            ContentType = "image/jpeg", // Thêm ContentType
            IsDeleted = false
        };
        _context.FileMetadata.Add(fileMetadata);
        await _context.SaveChangesAsync();

        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid()); // Current user is different

        var command = new DeleteFileCommand { FileId = fileId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied. You do not have permission to perform this action.");
        result.ErrorSource.Should().Be("Forbidden");

    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi quá trình xóa tệp khỏi dịch vụ lưu trữ tệp (IFileStorage) không thành công.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một FileMetadata hợp lệ. Cấu hình _mockUser.Id khớp với UploadedBy của FileMetadata.
    ///               Cấu hình _mockFileStorage.DeleteFileAsync để trả về Result.Failure.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thất bại (IsSuccess = false) và chứa thông báo lỗi phù hợp ("Storage deletion failed.").
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Lỗi từ dịch vụ lưu trữ tệp phải được truyền lại cho người gọi, cho biết rằng tệp không thể bị xóa hoàn toàn.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileStorageDeletionFails()
    {

        var fileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var fileMetadata = new FileMetadata
        {
            Id = fileId,
            FileName = "test.jpg",
            Url = "http://example.com/test.jpg",
            UploadedBy = userId.ToString(),
            ContentType = "image/jpeg", // Thêm ContentType
            IsDeleted = false
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

    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler xóa tệp và siêu dữ liệu của nó khỏi cơ sở dữ liệu thành công
    /// khi tất cả các điều kiện được đáp ứng (tìm thấy siêu dữ liệu, người dùng được ủy quyền, xóa khỏi storage thành công).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một FileMetadata hợp lệ. Cấu hình _mockUser.Id khớp với UploadedBy của FileMetadata.
    ///               Cấu hình _mockFileStorage.DeleteFileAsync để trả về Result.Success().
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thành công (IsSuccess = true).
    ///              Xác minh rằng _mockFileStorage.DeleteFileAsync được gọi một lần với URL của tệp.
    ///              Xác minh rằng FileMetadata đã bị xóa khỏi cơ sở dữ liệu.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Đây là kịch bản thành công, nơi tệp và tất cả các bản ghi liên quan của nó được xóa sạch khỏi hệ thống.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldDeleteFileAndMetadataSuccessfully()
    {

        var fileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var fileMetadata = new FileMetadata
        {
            Id = fileId,
            FileName = "test.jpg",
            Url = "http://example.com/test.jpg",
            UploadedBy = userId.ToString(),
            ContentType = "image/jpeg", // Thêm ContentType
            IsDeleted = false
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

    }
}
