using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UnitTests.Common;
using backend.Application.UserActivities.Queries;
using backend.Application.UserActivities.Queries.GetRecentActivities;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.UserActivities.Queries.GetRecentActivities;

public class GetRecentActivitiesQueryHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly GetRecentActivitiesQueryHandler _handler;

    public GetRecentActivitiesQueryHandlerTests()
    {
        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _handler = new GetRecentActivitiesQueryHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotAuthenticated()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về thất bại khi người dùng chưa được xác thực.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thiết lập _mockUser.Setup(u => u.Id).Returns((string)null);
        _mockUser.Setup(u => u.Id).Returns(string.Empty);
        var query = new GetRecentActivitiesQuery();

        // 2. Act: Gọi phương thức Handle của handler.
        var result = await _handler.Handle(query, CancellationToken.None);

        // 3. Assert: Kiểm tra rằng Result trả về là thất bại và chứa thông báo lỗi "User is not authenticated.".
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User is not authenticated.");
        // 💡 Giải thích: Handler phải kiểm tra xác thực người dùng trước khi xử lý truy vấn.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserProfileNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về thất bại khi không tìm thấy hồ sơ người dùng.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thiết lập _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid().ToString()); và _mockAuthorizationService để trả về null.
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserProfile));
        var query = new GetRecentActivitiesQuery();

        // 2. Act: Gọi phương thức Handle của handler.
        var result = await _handler.Handle(query, CancellationToken.None);

        // 3. Assert: Kiểm tra rằng Result trả về là thất bại và chứa thông báo lỗi "User profile not found.".
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
        // 💡 Giải thích: Handler phải kiểm tra sự tồn tại của hồ sơ người dùng sau khi xác thực.
    }

    [Fact]
    public async Task Handle_ShouldReturnRecentActivities_WhenActivitiesExist()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về danh sách các hoạt động gần đây khi chúng tồn tại.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thiết lập _mockUser và _mockAuthorizationService. Tạo và thêm UserActivity entities vào DB.
        var currentUserId = Guid.NewGuid().ToString();
        _mockUser.Setup(u => u.Id).Returns(currentUserId);

        var userProfileId = Guid.NewGuid();
        var userProfile = new UserProfile { Id = userProfileId, ExternalId = currentUserId, Email = "test@example.com", Name = "Test User" };
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);

        var activities = new List<UserActivity>
        {
            new() { Id = Guid.NewGuid(), UserProfileId = userProfileId, ActionType = UserActionType.CreateFamily, TargetType = TargetType.Family, TargetId = Guid.NewGuid().ToString(), GroupId = Guid.NewGuid(), ActivitySummary = "Activity 1" },
            new() { Id = Guid.NewGuid(), UserProfileId = userProfileId, ActionType = UserActionType.UpdateMember, TargetType = TargetType.Member, TargetId = Guid.NewGuid().ToString(), GroupId = Guid.NewGuid(), ActivitySummary = "Activity 2" },
            new() { Id = Guid.NewGuid(), UserProfileId = userProfileId, ActionType = UserActionType.DeleteEvent, TargetType = TargetType.Event, TargetId = Guid.NewGuid().ToString(), GroupId = Guid.NewGuid(), ActivitySummary = "Activity 3" },
            new() { Id = Guid.NewGuid(), UserProfileId = userProfileId, ActionType = UserActionType.CreateFamily, TargetType = TargetType.Family, TargetId = Guid.NewGuid().ToString(), GroupId = Guid.NewGuid(), ActivitySummary = "Activity 4" },
            new() { Id = Guid.NewGuid(), UserProfileId = userProfileId, ActionType = UserActionType.UpdateMember, TargetType = TargetType.Member, TargetId = Guid.NewGuid().ToString(), GroupId = Guid.NewGuid(), ActivitySummary = "Activity 5" }
        };
        _context.UserActivities.AddRange(activities);
        await _context.SaveChangesAsync();

        var query = new GetRecentActivitiesQuery { Limit = 3 };

        // 2. Act: Gọi phương thức Handle của handler.
        var result = await _handler.Handle(query, CancellationToken.None);

        // 3. Assert: Kiểm tra rằng Result trả về là thành công, chứa danh sách các UserActivityDto và số lượng hoạt động trả về khớp với Limit.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
        result.Value!.Select(dto => dto.UserProfileId).Should().AllBeEquivalentTo(userProfile.Id);
        // 💡 Giải thích: Handler phải áp dụng các thông số kỹ thuật và trả về các hoạt động được ánh xạ chính xác.
    }

    [Fact]
    public async Task Handle_ShouldFilterByTargetTypeAndTargetId()
    {
        // 🎯 Mục tiêu của test: Xác minh handler lọc hoạt động theo TargetType và TargetId.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thiết lập _mockUser và _mockAuthorizationService. Tạo và thêm UserActivity entities vào DB.
        var currentUserId = Guid.NewGuid().ToString();
        _mockUser.Setup(u => u.Id).Returns(currentUserId);

        var userProfileId = Guid.NewGuid();
        var userProfile = new UserProfile { Id = userProfileId, ExternalId = currentUserId, Email = "test@example.com", Name = "Test User" };
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);

        var targetFamilyId = Guid.NewGuid().ToString();
        var activities = new List<UserActivity>
        {
            _fixture.Build<UserActivity>()
                .With(ua => ua.UserProfileId, userProfileId)
                .With(ua => ua.TargetType, TargetType.Family)
                .With(ua => ua.TargetId, targetFamilyId)
                .Without(ua => ua.Metadata)
                .Create(),
            _fixture.Build<UserActivity>()
                .With(ua => ua.UserProfileId, userProfileId)
                .With(ua => ua.TargetType, TargetType.Member)
                .With(ua => ua.TargetId, Guid.NewGuid().ToString())
                .Without(ua => ua.Metadata)
                .Create(),
            _fixture.Build<UserActivity>()
                .With(ua => ua.UserProfileId, userProfileId)
                .With(ua => ua.TargetType, TargetType.Family)
                .With(ua => ua.TargetId, targetFamilyId)
                .Without(ua => ua.Metadata)
                .Create()
        };
        _context.UserActivities.AddRange(activities);
        await _context.SaveChangesAsync();

        var query = new GetRecentActivitiesQuery
        {
            TargetType = TargetType.Family,
            TargetId = targetFamilyId
        };

        // 2. Act: Gọi phương thức Handle của handler.
        var result = await _handler.Handle(query, CancellationToken.None);

        // 3. Assert: Kiểm tra rằng Result trả về là thành công và chỉ chứa các hoạt động khớp với TargetType và TargetId đã cho.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(dto =>
        {
            dto.TargetType.Should().Be(TargetType.Family);
            dto.TargetId.Should().Be(targetFamilyId);
        });
        // 💡 Giải thích: Handler phải áp dụng UserActivityByTargetSpec để lọc kết quả.
    }

    [Fact]
    public async Task Handle_ShouldFilterByGroupId()
    {
        // 🎯 Mục tiêu của test: Xác minh handler lọc hoạt động theo GroupId.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thiết lập _mockUser và _mockAuthorizationService. Tạo và thêm UserActivity entities vào DB.
        var currentUserId = Guid.NewGuid().ToString();
        _mockUser.Setup(u => u.Id).Returns(currentUserId);

        var userProfileId = Guid.NewGuid();
        var userProfile = new UserProfile { Id = userProfileId, ExternalId = currentUserId, Email = "test@example.com", Name = "Test User" };
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);

        var targetGroupId = Guid.NewGuid();
        var activities = new List<UserActivity>
        {
            _fixture.Build<UserActivity>()
                .With(ua => ua.UserProfileId, userProfileId)
                .With(ua => ua.GroupId, targetGroupId)
                .Without(ua => ua.Metadata)
                .Create(),
            _fixture.Build<UserActivity>()
                .With(ua => ua.UserProfileId, userProfileId)
                .With(ua => ua.GroupId, Guid.NewGuid())
                .Without(ua => ua.Metadata)
                .Create(),
            _fixture.Build<UserActivity>()
                .With(ua => ua.UserProfileId, userProfileId)
                .With(ua => ua.GroupId, targetGroupId)
                .Without(ua => ua.Metadata)
                .Create()
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
