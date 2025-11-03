
using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Queries;
using backend.Application.UserActivities.Queries.GetRecentActivities;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.UserActivities.Queries.GetRecentActivities;

public class GetRecentActivitiesQueryHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;

    public GetRecentActivitiesQueryHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedListOfUserActivities_ForCurrentUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var activities = new List<UserActivity>
        {
            new UserActivity(userId, UserActionType.Login.ToString(), "Logged in", TargetType.UserProfile, null, null, null),
            new UserActivity(userId, UserActionType.CreateFamily.ToString(), "Created family", TargetType.Family, Guid.NewGuid().ToString(), Guid.NewGuid(), null),
            new UserActivity(Guid.NewGuid(), UserActionType.Login.ToString(), "Other user logged in", TargetType.UserProfile, null, null, null)
        };
        _context.UserActivities.AddRange(activities);
        await _context.SaveChangesAsync();

        var handler = new GetRecentActivitiesQueryHandler(_context, _mapper, _currentUserMock.Object, _mockAuthorizationService.Object);
        var query = new GetRecentActivitiesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.All(x => x.UserId == userId).Should().BeTrue();
    }
}
