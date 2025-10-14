using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.UploadFile;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Files.UploadFile;

public class UploadFileCommandHandlerTests : TestBase
{
    private readonly UploadFileCommandHandler _handler;
    private readonly Mock<IFileStorage> _mockFileStorage;
    private readonly Mock<IOptions<StorageSettings>> _mockStorageSettings;
    private readonly Mock<IDateTime> _mockDateTime;

    public UploadFileCommandHandlerTests()
    {
        _mockFileStorage = new Mock<IFileStorage>();
        _mockStorageSettings = new Mock<IOptions<StorageSettings>>();
        _mockDateTime = new Mock<IDateTime>();
        _mockStorageSettings.Setup(s => s.Value.As<StorageSettings>()).Returns(new StorageSettings());
        _handler = new UploadFileCommandHandler(_mockFileStorage.Object, _mockStorageSettings.Object, _context, _mockUser.Object, _mockDateTime.Object);
    }

    /// <summary>
    /// Thiết lập môi trường kiểm thử bằng cách xóa dữ liệu cũ và tạo người dùng, hồ sơ người dùng, gia đình.
    /// </summary>
    /// <param name="userId">ID của người dùng hiện tại.</param>
    /// <param name="userProfileId">ID của hồ sơ người dùng.</param>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="isAdmin">Cho biết người dùng có phải là quản trị viên hay không.</param>
    /// <param name="canManageFamily">Cho biết người dùng có quyền quản lý gia đình hay không.</param>
    /// <param name="userProfileExists">Cho biết hồ sơ người dùng có tồn tại hay không.</param>
    private async Task ClearDatabaseAndSetupUser(string userId, Guid userProfileId, Guid familyId, bool isAdmin, bool canManageFamily, bool userProfileExists = true)
    {
        // Xóa tất cả dữ liệu liên quan để đảm bảo môi trường sạch cho mỗi bài kiểm tra.
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Members.RemoveRange(_context.Members);
        _context.Events.RemoveRange(_context.Events);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        _context.Relationships.RemoveRange(_context.Relationships);
        _context.UserActivities.RemoveRange(_context.UserActivities);
        _context.UserPreferences.RemoveRange(_context.UserPreferences);
        _context.FileMetadata.RemoveRange(_context.FileMetadata);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Thiết lập ID người dùng hiện tại cho mock IUser.
        _mockUser.Setup(x => x.Id).Returns(userId);

        if (userProfileExists)
        {
            // Tạo và thêm hồ sơ người dùng vào cơ sở dữ liệu.
            var userProfile = new UserProfile { Id = userProfileId, ExternalId = userId, Email = "test@example.com", Name = "Test User" };
            _context.UserProfiles.Add(userProfile);
            // Thiết lập người dùng với vai trò Quản lý gia đình.
            _context.FamilyUsers.Add(new FamilyUser { FamilyId = familyId, UserProfileId = userProfileId, Role = FamilyRole.Manager });
            await _context.SaveChangesAsync(CancellationToken.None);

            // Thiết lập các hành vi của mock IAuthorizationService.
            _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);
            _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(isAdmin);
            _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId, userProfile)).Returns(canManageFamily);
        }
        else
        {
            // Thiết lập mock IAuthorizationService trả về null nếu hồ sơ người dùng không tồn tại.
            _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserProfile)null!);
        }
    }

    /// <summary>
    /// Kiểm tra xem một tệp có được tải lên thành công.
    /// </summary>
    [Fact]
    public async Task Handle_Should_UploadFile_Successfully()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        _mockFileStorage.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("path/to/uploaded.jpg"));

        var command = new UploadFileCommand
        {
            FileName = "test.jpg",
            ContentType = "image/jpeg",
            FileStream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 })
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var uploadedFile = _context.FileMetadata.FirstOrDefault();
        uploadedFile.Should().NotBeNull();
        uploadedFile!.FileName.Should().Be(command.FileName);
        _mockFileStorage.Verify(s => s.UploadFileAsync(command.FileStream, command.FileName, command.ContentType, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Kiểm tra khi tệp không hợp lệ (ví dụ: nội dung rỗng).
    /// </summary>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_InvalidFile()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        var command = new UploadFileCommand
        {
            FileName = "invalid.jpg",
            ContentType = "image/jpeg",
            FileStream = new MemoryStream()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("File content cannot be empty.");
        _mockFileStorage.Verify(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Kiểm tra khi người dùng không được ủy quyền để tải lên tệp.
    /// </summary>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserIsNotAuthorized()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true, false); // userProfileExists = false

        var command = new UploadFileCommand
        {
            FileName = "unauthorized.jpg",
            ContentType = "image/jpeg",
            FileStream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 })
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
        _mockFileStorage.Verify(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
