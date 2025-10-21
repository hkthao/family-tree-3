using AutoFixture;
using AutoFixture.AutoMoq;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.CleanupUnusedFiles;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace backend.Application.UnitTests.Files.CleanupUnusedFiles;

public class CleanupUnusedFilesCommandHandlerTests : TestBase
{
    private readonly Mock<IFileStorage> _mockFileStorage;
    private readonly Mock<IDateTime> _mockDateTime;
    private readonly CleanupUnusedFilesCommandHandler _handler;

    public CleanupUnusedFilesCommandHandlerTests()
    {
        _mockFileStorage = new Mock<IFileStorage>();
        _mockDateTime = new Mock<IDateTime>();
        _fixture.Customize(new AutoMoqCustomization());

        _handler = new CleanupUnusedFilesCommandHandler(
            _context,
            _mockFileStorage.Object,
            _mockDateTime.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnZeroWhenNoUnusedFiles()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về 0 khi không có tệp nào cần dọn dẹp.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockDateTime.Now. Tạo một số FileMetadata nhưng không có cái nào đáp ứng tiêu chí 'unused'.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công với giá trị 0. Xác minh _fileStorage.DeleteFileAsync không được gọi.
        var now = DateTime.UtcNow;
        _mockDateTime.Setup(dt => dt.Now).Returns(now);

        var command = new CleanupUnusedFilesCommand { OlderThan = TimeSpan.FromDays(30) };

        // Add a file that is active (should not be deleted)
        _context.FileMetadata.Add(new FileMetadata
        {
            Id = Guid.NewGuid(),
            FileName = "active_file.jpg",
            Url = "http://example.com/active_file.jpg",
            UploadedBy = Guid.NewGuid().ToString(),
            ContentType = "image/jpeg",
            IsActive = true,
            Created = now.Subtract(TimeSpan.FromDays(40))
        });

        // Add a file that is used (should not be deleted)
        _context.FileMetadata.Add(new FileMetadata
        {
            Id = Guid.NewGuid(),
            FileName = "used_file.jpg",
            Url = "http://example.com/used_file.jpg",
            UploadedBy = Guid.NewGuid().ToString(),
            ContentType = "image/jpeg",
            IsActive = false,
            UsedById = Guid.NewGuid(), // Used by some entity
            Created = now.Subtract(TimeSpan.FromDays(40))
        });

        // Add a file that is not old enough (should not be deleted)
        _context.FileMetadata.Add(new FileMetadata
        {
            Id = Guid.NewGuid(),
            FileName = "recent_file.jpg",
            Url = "http://example.com/recent_file.jpg",
            UploadedBy = Guid.NewGuid().ToString(),
            ContentType = "image/jpeg",
            IsActive = false,
            UsedById = null,
            Created = now.Subtract(TimeSpan.FromDays(10)) // Not older than 30 days
        });

        await _context.SaveChangesAsync();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        _mockFileStorage.Verify(fs => fs.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _context.FileMetadata.Count().Should().Be(3); // No files should be deleted from DB
        // 💡 Giải thích: Không có tệp nào đáp ứng tất cả các tiêu chí để được dọn dẹp, vì vậy không có tệp nào bị xóa và số lượng tệp đã xóa là 0.
    }

    [Fact]
    public async Task Handle_ShouldDeleteUnusedFilesSuccessfully()
    {
        // 🎯 Mục tiêu của test: Xác minh handler xóa thành công các tệp không sử dụng khỏi bộ lưu trữ và cơ sở dữ liệu.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockDateTime.Now. Tạo một số FileMetadata đáp ứng tiêu chí 'unused'. Mock _fileStorage.DeleteFileAsync trả về Result.Success().
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công với số lượng tệp đã xóa chính xác. Xác minh _fileStorage.DeleteFileAsync được gọi cho mỗi tệp không sử dụng. Xác minh các FileMetadata đã bị xóa khỏi Context.
        var now = DateTime.UtcNow;
        _mockDateTime.Setup(dt => dt.Now).Returns(now);

        var command = new CleanupUnusedFilesCommand { OlderThan = TimeSpan.FromDays(30) };

        var unusedFile1 = new FileMetadata
        {
            Id = Guid.NewGuid(),
            FileName = "unused_file1.jpg",
            Url = "http://example.com/unused_file1.jpg",
            UploadedBy = Guid.NewGuid().ToString(),
            ContentType = "image/jpeg",
            IsActive = false,
            UsedById = null,
            Created = now.Subtract(TimeSpan.FromDays(40)) // Older than 30 days
        };
        var unusedFile2 = new FileMetadata
        {
            Id = Guid.NewGuid(),
            FileName = "unused_file2.png",
            Url = "http://example.com/unused_file2.png",
            UploadedBy = Guid.NewGuid().ToString(),
            ContentType = "image/png",
            IsActive = false,
            UsedById = null,
            Created = now.Subtract(TimeSpan.FromDays(50)) // Older than 30 days
        };

        // Add a file that should NOT be deleted (e.g., active)
        var activeFile = new FileMetadata
        {
            Id = Guid.NewGuid(),
            FileName = "active_file.gif",
            Url = "http://example.com/active_file.gif",
            UploadedBy = Guid.NewGuid().ToString(),
            ContentType = "image/gif",
            IsActive = true,
            UsedById = null,
            Created = now.Subtract(TimeSpan.FromDays(60))
        };

        _context.FileMetadata.AddRange(unusedFile1, unusedFile2, activeFile);
        await _context.SaveChangesAsync();

        _mockFileStorage.Setup(fs => fs.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2); // Two files should be deleted

        _mockFileStorage.Verify(fs => fs.DeleteFileAsync(unusedFile1.Url, It.IsAny<CancellationToken>()), Times.Once);
        _mockFileStorage.Verify(fs => fs.DeleteFileAsync(unusedFile2.Url, It.IsAny<CancellationToken>()), Times.Once);
        _mockFileStorage.Verify(fs => fs.DeleteFileAsync(activeFile.Url, It.IsAny<CancellationToken>()), Times.Never);

        _context.FileMetadata.Should().ContainSingle(fm => fm.Id == activeFile.Id); // Only active file should remain
        _context.FileMetadata.Should().NotContain(fm => fm.Id == unusedFile1.Id);
        _context.FileMetadata.Should().NotContain(fm => fm.Id == unusedFile2.Id);
        // 💡 Giải thích: Hai tệp không sử dụng đã được xóa thành công khỏi bộ lưu trữ và cơ sở dữ liệu, trong khi tệp đang hoạt động vẫn còn nguyên.
    }

    [Fact]
    public async Task Handle_ShouldNotDeleteMetadataWhenFileStorageDeletionFails()
    {
        // 🎯 Mục tiêu của test: Xác minh rằng siêu dữ liệu không bị xóa khỏi DB nếu xóa tệp khỏi bộ lưu trữ thất bại.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockDateTime.Now. Tạo một FileMetadata đáp ứng tiêu chí 'unused'. Mock _fileStorage.DeleteFileAsync trả về Result.Failure.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công với giá trị 0. Xác minh _fileStorage.DeleteFileAsync được gọi. Xác minh FileMetadata vẫn còn trong Context.
        var now = DateTime.UtcNow;
        _mockDateTime.Setup(dt => dt.Now).Returns(now);

        var command = new CleanupUnusedFilesCommand { OlderThan = TimeSpan.FromDays(30) };

        var unusedFile = new FileMetadata
        {
            Id = Guid.NewGuid(),
            FileName = "failed_delete_file.jpg",
            Url = "http://example.com/failed_delete_file.jpg",
            UploadedBy = Guid.NewGuid().ToString(),
            ContentType = "image/jpeg",
            IsActive = false,
            UsedById = null,
            Created = now.Subtract(TimeSpan.FromDays(40)) // Older than 30 days
        };

        _context.FileMetadata.Add(unusedFile);
        await _context.SaveChangesAsync();

        _mockFileStorage.Setup(fs => fs.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Storage deletion failed.", "FileStorage"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(); // Handler returns success even if some deletions fail, but count is 0 for failed ones
        result.Value.Should().Be(0); // No files successfully deleted from DB

        _mockFileStorage.Verify(fs => fs.DeleteFileAsync(unusedFile.Url, It.IsAny<CancellationToken>()), Times.Once);
        _context.FileMetadata.Should().ContainSingle(fm => fm.Id == unusedFile.Id); // Metadata should still be in DB
        // 💡 Giải thích: Mặc dù tệp được xác định là không sử dụng, nhưng do lỗi khi xóa khỏi bộ lưu trữ, siêu dữ liệu của nó không được xóa khỏi cơ sở dữ liệu.
    }
}
