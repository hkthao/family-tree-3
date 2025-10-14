using backend.Application.Common.Models;
using backend.Application.Files.Queries.GetUploadedFile;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Files.Queries.GetUploadedFile;

public class GetUploadedFileQueryHandlerTests : TestBase
{
    private readonly GetUploadedFileQueryHandler _handler;
    private readonly Mock<IOptions<StorageSettings>> _mockStorageSettings;
    

    public GetUploadedFileQueryHandlerTests()
    {
        _mockStorageSettings = new Mock<IOptions<StorageSettings>>();
        _mockStorageSettings.Setup(s => s.Value).Returns(new StorageSettings
        {
            Local = new LocalStorageSettings
            {
                LocalStoragePath = Path.Combine(Path.GetTempPath(), "test_storage")
            }
        });
        _handler = new GetUploadedFileQueryHandler(_mockStorageSettings.Object);
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
    /// Kiểm tra xem một tệp có được trả về thành công khi tìm thấy.
    /// </summary>
    [Fact]
    public async Task Handle_Should_ReturnFile_When_Found()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        var uploadedFile = new FileMetadata { Id = Guid.NewGuid(), FileName = "test.jpg", Url = "path/to/test.jpg", UploadedBy = userProfileId.ToString(), ContentType = "image/jpeg" };
        _context.FileMetadata.Add(uploadedFile);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Create a dummy file in the mocked storage path
        var localStoragePath = _mockStorageSettings.Object.Value.Local.LocalStoragePath;
        Directory.CreateDirectory(localStoragePath);
        var filePath = Path.Combine(localStoragePath, uploadedFile.FileName);
        await File.WriteAllBytesAsync(filePath, new byte[] { 0x01, 0x02, 0x03 });

        var query = new GetUploadedFileQuery { FileName = uploadedFile.FileName };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ContentType.Should().Be(uploadedFile.ContentType);

        // Clean up the dummy file and directory
        File.Delete(filePath);
        Directory.Delete(localStoragePath);
    }

    /// <summary>
    /// Kiểm tra khi không tìm thấy tệp.
    /// </summary>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_FileNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        var nonExistentFileId = Guid.NewGuid();
        var query = new GetUploadedFileQuery { FileName = "nonexistent.jpg" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("File not found.");
    }
}
