using backend.Application.Identity.UserProfiles.Commands.SyncUserProfile;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Commands.SyncUserProfile;

public class SyncUserProfileCommandHandlerTests : TestBase
{
    private readonly SyncUserProfileCommandHandler _handler;
    private readonly Mock<ILogger<SyncUserProfileCommandHandler>> _mockLogger;

    public SyncUserProfileCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<SyncUserProfileCommandHandler>>();
        _handler = new SyncUserProfileCommandHandler(_context, _mockLogger.Object);
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
    /// Kiểm tra xem hồ sơ người dùng mới có được tạo thành công khi không tìm thấy hồ sơ hiện có.
    /// </summary>
    [Fact]
    public async Task Handle_Should_CreateUserProfile_When_NotFound()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true, false); // userProfileExists = false

        var command = new SyncUserProfileCommand
        {
            UserPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, "newuser@example.com"),
                new Claim(ClaimTypes.Name, "New User")
            }, "mock"))
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var userProfile = _context.UserProfiles.FirstOrDefault();
        userProfile.Should().NotBeNull();
    }

    /// <summary>
    /// Kiểm tra xem hồ sơ người dùng hiện có có được cập nhật thành công.
    /// </summary>
    [Fact]
    public async Task Handle_Should_UpdateUserProfile_When_Found()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        var command = new SyncUserProfileCommand
        {
            UserPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, "updated@example.com"),
                new Claim(ClaimTypes.Name, "Updated User")
            }, "mock"))
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var userProfile = _context.UserProfiles.FirstOrDefault();
        userProfile.Should().NotBeNull();
    }

    /// <summary>
    /// Kiểm tra khi ExternalId không hợp lệ (null hoặc rỗng).
    /// </summary>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_InvalidExternalId()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        var command = new SyncUserProfileCommand
        {
            UserPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "invalid@example.com"),
                new Claim(ClaimTypes.Name, "Invalid User")
            }, "mock"))
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("External ID (sub claim) not found in claims.");
    }
}
