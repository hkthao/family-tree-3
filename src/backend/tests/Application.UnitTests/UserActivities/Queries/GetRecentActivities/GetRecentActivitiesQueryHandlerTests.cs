using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.UnitTests.Common;
using backend.Application.UserActivities.Queries.GetRecentActivities;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
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


    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserProfileNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về thất bại khi không tìm thấy hồ sơ người dùng.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thiết lập _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid().ToString()); và đảm bảo không có UserProfile nào trong DB.
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        // Ensure no UserProfile exists for this ID
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);
        var query = new GetRecentActivitiesQuery();

        // 2. Act: Gọi phương thức Handle của handler.
        var result = await _handler.Handle(query, CancellationToken.None);

        // 3. Assert: Kiểm tra rằng Result trả về là thất bại và chứa thông báo lỗi "User profile not found.".
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        // 💡 Giải thích: Handler phải kiểm tra sự tồn tại của hồ sơ người dùng sau khi xác thực.
    }





    [Fact]
    public async Task Handle_ShouldFilterByGroupId()
    {
        // 🎯 Mục tiêu của test: Xác minh handler lọc hoạt động theo GroupId.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thiết lập _mockUser và _mockAuthorizationService. Tạo và thêm UserActivity entities vào DB.
        _context.UserActivities.RemoveRange(_context.UserActivities);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);
        var currentUserId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(currentUserId);

        var userProfile = new UserProfile { Id = currentUserId, ExternalId = currentUserId.ToString(), Email = "test@example.com", Name = "Test User" };
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync(CancellationToken.None);

        var targetGroupId = Guid.NewGuid();
        var activities = new List<UserActivity>
        {
            new() { Id = Guid.NewGuid(), UserProfileId = currentUserId, ActionType = UserActionType.CreateFamily, TargetType = TargetType.Family, TargetId = Guid.NewGuid().ToString(), GroupId = targetGroupId, ActivitySummary = "Activity 1" },
            new() { Id = Guid.NewGuid(), UserProfileId = currentUserId, ActionType = UserActionType.UpdateMember, TargetType = TargetType.Member, TargetId = Guid.NewGuid().ToString(), GroupId = Guid.NewGuid(), ActivitySummary = "Activity 2" },
            new() { Id = Guid.NewGuid(), UserProfileId = currentUserId, ActionType = UserActionType.DeleteEvent, TargetType = TargetType.Event, TargetId = Guid.NewGuid().ToString(), GroupId = targetGroupId, ActivitySummary = "Activity 3" }
        };
        _context.UserActivities.AddRange(activities);
        await _context.SaveChangesAsync();

        var query = new GetRecentActivitiesQuery
        {
            GroupId = targetGroupId
        };

        // 2. Act: Gọi phương thức Handle của handler.
        var result = await _handler.Handle(query, CancellationToken.None);

        // 3. Assert: Kiểm tra rằng Result trả về là thành công và chỉ chứa các hoạt động khớp với GroupId đã cho.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(dto => dto.GroupId.Should().Be(targetGroupId));
        // 💡 Giải thích: Handler phải áp dụng UserActivityByGroupSpec để lọc kết quả.
    }
}
