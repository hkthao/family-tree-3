using Microsoft.EntityFrameworkCore;
using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.CleanupUnusedFiles;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Files.CleanupUnusedFiles;

public class CleanupUnusedFilesCommandHandlerTests : TestBase
{
    private readonly Mock<IFileStorage> _mockFileStorage;
    private readonly CleanupUnusedFilesCommandHandler _handler;

    public CleanupUnusedFilesCommandHandlerTests()
    {
        _mockFileStorage = new Mock<IFileStorage>();

        _handler = new CleanupUnusedFilesCommandHandler(_context, _mockFileStorage.Object, _mockDateTime.Object);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về thành công với số lượng file đã xóa là 0
    /// khi không tìm thấy file không sử dụng nào trong cơ sở dữ liệu.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập thời gian hiện tại của hệ thống. Đảm bảo cơ sở dữ liệu không chứa FileMetadata nào thỏa mãn điều kiện xóa.
    ///    - Act: Gọi phương thức Handle của handler với một CleanupUnusedFilesCommand.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thành công và giá trị là 0.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Khi không có file nào để dọn dẹp, hệ thống nên báo cáo thành công
    /// mà không thực hiện bất kỳ thao tác xóa nào.
    /// </summary>
    [Fact]
    public async Task Handle_NoUnusedFilesFound_ReturnsSuccessWithZeroDeleted()
    {
        // Arrange
        var now = _fixture.Create<DateTime>();
        _mockDateTime.Setup(dt => dt.Now).Returns(now);
        var command = new CleanupUnusedFilesCommand { OlderThan = TimeSpan.FromDays(30) };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
        _mockFileStorage.Verify(fs => fs.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal(0, _context.FileMetadata.Count());
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler xóa thành công các file không sử dụng
    /// và trả về số lượng file đã xóa chính xác.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập thời gian hiện tại. Thêm các FileMetadata không sử dụng vào cơ sở dữ liệu.
    ///               Cấu hình _mockFileStorage để trả về thành công khi xóa file.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thành công và số lượng file đã xóa là chính xác.
    ///              Xác minh rằng _mockFileStorage.DeleteFileAsync được gọi cho mỗi file.
    ///              Xác minh rằng các file đã bị xóa khỏi cơ sở dữ liệu.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Khi có các file không sử dụng và việc xóa khỏi storage thành công,
    /// hệ thống nên xóa chúng khỏi cả storage và cơ sở dữ liệu, đồng thời báo cáo số lượng chính xác.
    /// </summary>
    // [Fact]
    // public async Task Handle_UnusedFilesFoundAndSuccessfullyDeleted_ReturnsSuccessWithCorrectCount()
    // {
    //     // Arrange
    //     var now = _fixture.Create<DateTime>();
    //     _mockDateTime.Setup(dt => dt.Now).Returns(now);
    //     var cutoffDate = now.Subtract(TimeSpan.FromDays(30));
    //     var command = new CleanupUnusedFilesCommand { OlderThan = TimeSpan.FromDays(30) };

    //     var unusedFile1 = new FileMetadata
    //     {
    //         Id = Guid.NewGuid(),
    //         Url = "file1.jpg",
    //         FileName = "file1.jpg",
    //         ContentType = "image/jpeg",
    //         UploadedBy = Guid.NewGuid().ToString(),
    //         IsDeleted = false,
    //         UsedById = null,
    //         Created = cutoffDate.Subtract(TimeSpan.FromDays(100))
    //     };
    //     var unusedFile2 = new FileMetadata
    //     {
    //         Id = Guid.NewGuid(),
    //         Url = "file2.jpg",
    //         FileName = "file2.jpg",
    //         ContentType = "image/jpeg",
    //         UploadedBy = Guid.NewGuid().ToString(),
    //         IsDeleted = false,
    //         UsedById = null,
    //         Created = cutoffDate.Subtract(TimeSpan.FromDays(110))
    //     };
    //     var usedFile = _fixture.Build<FileMetadata>()
    //         .With(fm => fm.IsDeleted, true) // Active file
    //         .With(fm => fm.UsedById, _fixture.Create<Guid>())
    //         .With(fm => fm.Created, cutoffDate.Subtract(TimeSpan.FromDays(5)))
    //         .Create();
    //     var newFile = _fixture.Build<FileMetadata>()
    //         .With(fm => fm.IsDeleted, false)
    //         .With(fm => fm.UsedById, (Guid?)null)
    //         .With(fm => fm.Created, cutoffDate.AddDays(1)) // Newer than cutoff
    //         .Create();

    //     _context.FileMetadata.AddRange(unusedFile1, unusedFile2);
    //     await _context.SaveChangesAsync(CancellationToken.None);

    //     _mockFileStorage.Setup(fs => fs.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
    //         .ReturnsAsync(Result.Success());

    //     // Act
    //     var result = await _handler.Handle(command, CancellationToken.None);

    //     // Assert
    //     Assert.True(result.IsSuccess);
    //     Assert.Equal(2, result.Value); // unusedFile1 and unusedFile2 should be deleted

    //     _mockFileStorage.Verify(fs => fs.DeleteFileAsync(unusedFile1.Url, It.IsAny<CancellationToken>()), Times.Once);
    //     _mockFileStorage.Verify(fs => fs.DeleteFileAsync(unusedFile2.Url, It.IsAny<CancellationToken>()), Times.Once);

    //     Assert.Equal(2, _context.FileMetadata.IgnoreQueryFilters().Count()); // All files should still be in DB, but some soft-deleted
    //     Assert.True(_context.FileMetadata.IgnoreQueryFilters().First(fm => fm.Id == unusedFile1.Id).IsDeleted);
    //     Assert.True(_context.FileMetadata.IgnoreQueryFilters().First(fm => fm.Id == unusedFile2.Id).IsDeleted);
    // }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler xử lý đúng khi một số file không sử dụng không thể xóa khỏi storage.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập thời gian hiện tại. Thêm các FileMetadata không sử dụng vào cơ sở dữ liệu.
    ///               Cấu hình _mockFileStorage để trả về thành công cho một số file và thất bại cho các file khác.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thành công và số lượng file đã xóa là chính xác (chỉ những file xóa thành công).
    ///              Xác minh rằng _mockFileStorage.DeleteFileAsync được gọi cho tất cả các file không sử dụng.
    ///              Xác minh rằng chỉ những file xóa thành công mới bị xóa khỏi cơ sở dữ liệu.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Hệ thống nên tiếp tục xử lý các file khác ngay cả khi một số file không thể xóa.
    /// Chỉ những file được xóa thành công khỏi storage mới nên bị xóa khỏi cơ sở dữ liệu để duy trì tính nhất quán.
    /// </summary>
    // [Fact]
    // public async Task Handle_UnusedFilesFoundButPartialDeletionFails_ReturnsSuccessWithCorrectCount()
    // {
    //     // Arrange
    //     var now = _fixture.Create<DateTime>();
    //     _mockDateTime.Setup(dt => dt.Now).Returns(now);
    //     var cutoffDate = now.Subtract(TimeSpan.FromDays(30));
    //     var command = new CleanupUnusedFilesCommand { OlderThan = TimeSpan.FromDays(30) };

    //     var unusedFileSuccess1 = new FileMetadata
    //     {
    //         Id = Guid.NewGuid(),
    //         Url = "file_success1.jpg",
    //         FileName = "file_success1.jpg",
    //         ContentType = "image/jpeg",
    //         UploadedBy = Guid.NewGuid().ToString(),
    //         IsDeleted = false,
    //         UsedById = null,
    //         Created = cutoffDate.Subtract(TimeSpan.FromDays(101))
    //     };
    //     var unusedFileFail = new FileMetadata
    //     {
    //         Id = Guid.NewGuid(),
    //         Url = "file_fail.jpg",
    //         FileName = "file_fail.jpg",
    //         ContentType = "image/jpeg",
    //         UploadedBy = Guid.NewGuid().ToString(),
    //         IsDeleted = false,
    //         UsedById = null,
    //         Created = cutoffDate.Subtract(TimeSpan.FromDays(105))
    //     };
    //     var unusedFileSuccess2 = new FileMetadata
    //     {
    //         Id = Guid.NewGuid(),
    //         Url = "file_success2.jpg",
    //         FileName = "file_success2.jpg",
    //         ContentType = "image/jpeg",
    //         UploadedBy = Guid.NewGuid().ToString(),
    //         IsDeleted = false,
    //         UsedById = null,
    //         Created = cutoffDate.Subtract(TimeSpan.FromDays(110))
    //     };

    //     _context.FileMetadata.AddRange(unusedFileSuccess1, unusedFileFail, unusedFileSuccess2);
    //     await _context.SaveChangesAsync(CancellationToken.None);

    //     _mockFileStorage.SetupSequence(fs => fs.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
    //         .ReturnsAsync(Result.Success()) // For unusedFileSuccess1
    //         .ReturnsAsync(Result.Failure("Failed to delete")) // For unusedFileFail
    //         .ReturnsAsync(Result.Success()); // For unusedFileSuccess2

    //     // Act
    //     var result = await _handler.Handle(command, CancellationToken.None);

    //     // Assert
    //     Assert.True(result.IsSuccess);
    //     Assert.Equal(2, result.Value); // unusedFileSuccess1 and unusedFileSuccess2 should be deleted

    //     _mockFileStorage.Verify(fs => fs.DeleteFileAsync(unusedFileSuccess1.Url, It.IsAny<CancellationToken>()), Times.Once);
    //     _mockFileStorage.Verify(fs => fs.DeleteFileAsync(unusedFileFail.Url, It.IsAny<CancellationToken>()), Times.Once);
    //     _mockFileStorage.Verify(fs => fs.DeleteFileAsync(unusedFileSuccess2.Url, It.IsAny<CancellationToken>()), Times.Once);

    //     Assert.Equal(3, _context.FileMetadata.IgnoreQueryFilters().Count()); // All files should still be in DB, but some soft-deleted
    //     Assert.True(_context.FileMetadata.IgnoreQueryFilters().First(fm => fm.Id == unusedFileSuccess1.Id).IsDeleted);
    //     Assert.False(_context.FileMetadata.IgnoreQueryFilters().First(fm => fm.Id == unusedFileFail.Id).IsDeleted);
    //     Assert.True(_context.FileMetadata.IgnoreQueryFilters().First(fm => fm.Id == unusedFileSuccess2.Id).IsDeleted);
    // }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler không xóa bất kỳ file nào
    /// khi không có file nào cũ hơn ngày cắt (cutoff date).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập thời gian hiện tại. Thêm các FileMetadata không sử dụng vào cơ sở dữ liệu,
    ///               nhưng tất cả đều mới hơn hoặc bằng ngày cắt.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thành công và số lượng file đã xóa là 0.
    ///              Xác minh rằng _mockFileStorage.DeleteFileAsync không bao giờ được gọi.
    ///              Xác minh rằng tất cả các file ban đầu vẫn còn trong cơ sở dữ liệu.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Chỉ những file cũ hơn ngày cắt mới đủ điều kiện để xóa.
    /// Nếu không có file nào thỏa mãn điều kiện này, không có thao tác xóa nào nên được thực hiện.
    /// </summary>
    [Fact]
    public async Task Handle_NoFilesOlderThanCutoffDate_ReturnsSuccessWithZeroDeleted()
    {
        // Arrange
        var now = _fixture.Create<DateTime>();
        _mockDateTime.Setup(dt => dt.Now).Returns(now);
        var cutoffDate = now.Subtract(TimeSpan.FromDays(30));
        var command = new CleanupUnusedFilesCommand { OlderThan = TimeSpan.FromDays(30) };

        var unusedFileNotOldEnough1 = _fixture.Build<FileMetadata>()
            .With(fm => fm.IsDeleted, false)
            .With(fm => fm.UsedById, (Guid?)null)
            .With(fm => fm.Created, cutoffDate.AddDays(5)) // Newer than cutoff
            .Create();
        var unusedFileNotOldEnough2 = _fixture.Build<FileMetadata>()
            .With(fm => fm.IsDeleted, false)
            .With(fm => fm.UsedById, (Guid?)null)
            .With(fm => fm.Created, cutoffDate) // Exactly at cutoff
            .Create();

        _context.FileMetadata.AddRange(unusedFileNotOldEnough1, unusedFileNotOldEnough2);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);

        _mockFileStorage.Verify(fs => fs.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        Assert.Equal(2, _context.FileMetadata.IgnoreQueryFilters().Count());
        Assert.False(_context.FileMetadata.IgnoreQueryFilters().First(fm => fm.Id == unusedFileNotOldEnough1.Id).IsDeleted);
        Assert.False(_context.FileMetadata.IgnoreQueryFilters().First(fm => fm.Id == unusedFileNotOldEnough2.Id).IsDeleted);
    }
}
