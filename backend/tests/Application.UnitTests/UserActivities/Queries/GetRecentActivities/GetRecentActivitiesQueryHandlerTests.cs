using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Queries.GetRecentActivities;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.UserActivities.Queries.GetRecentActivities;

public class GetRecentActivitiesQueryHandlerTests : TestBase
{
    private readonly GetRecentActivitiesQueryHandler _handler;

    public GetRecentActivitiesQueryHandlerTests()
    {
        _handler = new GetRecentActivitiesQueryHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object);
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
    /// Kiểm tra xem các hoạt động gần đây có được trả về thành công.
    /// </summary>
    [Fact]
    public async Task Handle_Should_ReturnRecentActivities_Successfully()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        // Thêm một số hoạt động người dùng.
        _context.UserActivities.Add(new UserActivity { Id = Guid.NewGuid(), UserProfileId = userProfileId, ActionType = UserActionType.CreateFamily, TargetType = TargetType.Family, TargetId = familyId.ToString(), ActivitySummary = "Created Family 1", Created = DateTime.UtcNow.AddDays(-2) });
        _context.UserActivities.Add(new UserActivity { Id = Guid.NewGuid(), UserProfileId = userProfileId, ActionType = UserActionType.UpdateFamily, TargetType = TargetType.Family, TargetId = familyId.ToString(), ActivitySummary = "Updated Family 1", Created = DateTime.UtcNow.AddDays(-1) });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetRecentActivitiesQuery { Limit = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value!.First().ActivitySummary.Should().Be("Updated Family 1"); // Should be ordered by Created descending
    }

    /// <summary>
    /// Kiểm tra khi không có hoạt động nào được tìm thấy.
    /// </summary>
    [Fact]
    public async Task Handle_Should_ReturnEmptyList_WhenNoActivities()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        var query = new GetRecentActivitiesQuery { Limit = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }

    /// <summary>
    /// Kiểm tra phân trang khi truy xuất các hoạt động gần đây.
    /// </summary>
    [Fact]
    public async Task Handle_Should_ReturnPaginatedActivities()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        for (int i = 0; i < 5; i++)
        {
            _context.UserActivities.Add(new UserActivity { Id = Guid.NewGuid(), UserProfileId = userProfileId, ActionType = UserActionType.CreateFamily, TargetType = TargetType.Family, TargetId = familyId.ToString(), ActivitySummary = $"Activity {i}", Created = DateTime.UtcNow.AddDays(-i) });
        }
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetRecentActivitiesQuery { Limit = 2 }; // Use Limit for pagination

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        // GetRecentActivitiesQueryHandler returns List<UserActivityDto>, not PaginatedList<UserActivityDto>
        // So, TotalItems, Page, TotalPages are not available.
        // result.Value.TotalItems.Should().Be(5);
        // result.Value.Page.Should().Be(1);
        // result.Value.TotalPages.Should().Be(3);
        result.Value!.First().ActivitySummary.Should().Be("Activity 0"); // Oldest activity (assuming handler orders ascending)
    }
}
