using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Identity.Commands.UpdateUserProfile;
using backend.Application.Identity.UserProfiles.Commands.UpdateUserProfile;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandHandlerTests : TestBase
{
    private readonly UpdateUserProfileCommandHandler _handler;

    public UpdateUserProfileCommandHandlerTests()
    {
        _handler = new backend.Application.Identity.Commands.UpdateUserProfile.UpdateUserProfileCommandHandler(_context);
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
    /// Kiểm tra xem hồ sơ người dùng có được cập nhật thành công.
    /// </summary>
    [Fact]
    public async Task Handle_Should_UpdateUserProfile_Successfully()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        var command = new UpdateUserProfileCommand
        {
            Id = userProfileId.ToString(),
            Email = "updated@example.com",
            Name = "Updated User"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var userProfile = await _context.UserProfiles.FindAsync(userProfileId);
        userProfile.Should().NotBeNull();
        userProfile!.Email.Should().Be(command.Email);
        userProfile.Name.Should().Be(command.Name);
    }

    /// <summary>
    /// Kiểm tra khi hồ sơ người dùng không tồn tại.
    /// </summary>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserProfileNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true, false); // userProfileExists = false

        var command = new UpdateUserProfileCommand
        {
            Id = userProfileId.ToString(),
            Email = "updated@example.com",
            Name = "Updated User"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
    }

    /// <summary>
    /// Kiểm tra khi người dùng không được ủy quyền để cập nhật hồ sơ.
    /// </summary>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserIsNotAuthorized()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        // Mock IAuthorizationService để trả về rằng người dùng không được ủy quyền.
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString() }); // Khác với userProfileId

        var command = new UpdateUserProfileCommand
        {
            Id = userProfileId.ToString(),
            Email = "unauthorized@example.com",
            Name = "Unauthorized User"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User is not authorized to update this profile.");
    }
}
