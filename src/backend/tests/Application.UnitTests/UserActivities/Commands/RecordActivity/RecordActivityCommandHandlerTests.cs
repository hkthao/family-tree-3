
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.UserActivities.Commands.RecordActivity;

public class RecordActivityCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test_auth_id", "test@example.com") { Id = userId };
        user.UpdateProfile("test_external_id", "test@example.com", "Test User", "Test", "User", "123456789", "avatar.png");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Clear change tracker to simulate a fresh load
        _context.ChangeTracker.Clear();

        var handler = new RecordActivityCommandHandler(_context);
        var command = new RecordActivityCommand
        {
            UserId = userId,
            ActionType = UserActionType.Login,
            TargetType = TargetType.UserProfile,
            ActivitySummary = "User logged in"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        var activity = _context.UserActivities.FirstOrDefault();
        activity.Should().NotBeNull();
        activity!.UserId.Should().Be(userId);
        activity.ActionType.ToString().Should().Be(command.ActionType.ToString());
        activity.ActivitySummary.Should().Be(command.ActivitySummary);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var handler = new RecordActivityCommandHandler(_context);
        var command = new RecordActivityCommand
        {
            UserId = Guid.NewGuid(),
            ActionType = UserActionType.Login,
            TargetType = TargetType.UserProfile,
            ActivitySummary = "User logged in"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
