using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Identity.UserProfiles.Queries.GetAllUserProfiles;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Queries.GetAllUserProfiles;

public class GetAllUserProfilesQueryHandlerTests : TestBase
{
    private readonly GetAllUserProfilesQueryHandler _handler;

    public GetAllUserProfilesQueryHandlerTests()
    {
        _handler = new GetAllUserProfilesQueryHandler(_context, _mapper);
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
    /// Kiểm tra xem tất cả các hồ sơ người dùng có được trả về thành công.
    /// </summary>
    [Fact]
    public async Task Handle_Should_ReturnAllUserProfiles_Successfully()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        // Thêm một số hồ sơ người dùng.
        _context.UserProfiles.Add(new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "user1@example.com", Name = "User 1" });
        _context.UserProfiles.Add(new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "user2@example.com", Name = "User 2" });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetAllUserProfilesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(3); // 1 from setup, 2 added here
    }

    /// <summary>
    /// Kiểm tra khi không có hồ sơ người dùng nào được tìm thấy.
    /// </summary>
    [Fact]
    public async Task Handle_Should_ReturnEmptyList_WhenNoUserProfiles()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true, false); // userProfileExists = false

        var query = new GetAllUserProfilesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }
}
