
using backend.Application.Common.Interfaces;
using backend.Application.Identity.Commands.CreateNovuSubscriber;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Identity.Commands;

public class CreateNovuSubscriberCommandHandlerTests : TestBase
{
    private readonly Mock<INotificationProvider> _notificationProviderMock;
    private readonly Mock<ILogger<CreateNovuSubscriberCommandHandler>> _loggerMock;
    private readonly CreateNovuSubscriberCommandHandler _handler;

    public CreateNovuSubscriberCommandHandlerTests()
    {
        _notificationProviderMock = new Mock<INotificationProvider>();
        _loggerMock = new Mock<ILogger<CreateNovuSubscriberCommandHandler>>();
        _handler = new CreateNovuSubscriberCommandHandler(_context, _notificationProviderMock.Object, _loggerMock.Object);
    }

    private User CreateTestUser(string authId, string email)
    {
        return new User(authId, email, "Test User", "Test", "User", "123456", "avatar.png");
    }

    [Fact]
    public async Task Handle_ShouldSyncSubscriber_WhenUserHasNoSubscriberId()
    {
        // Arrange
        var user = CreateTestUser("auth0|test1", "test1@test.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new CreateNovuSubscriberCommand { UserId = user.Id };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _notificationProviderMock.Verify(p => p.SyncSubscriberAsync(user.Id.ToString(), It.IsAny<string>(), It.IsAny<string>(), user.Email, It.IsAny<string>()), Times.Once);
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.SubscriberId.Should().Be(user.Id.ToString());
    }

    [Fact]
    public async Task Handle_ShouldNotSyncSubscriber_WhenUserAlreadyHasSubscriberId()
    {
        // Arrange
        var user = CreateTestUser("auth0|test2", "test2@test.com");
        user.SubscriberId = user.Id.ToString(); // User already has a subscriber ID
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new CreateNovuSubscriberCommand { UserId = user.Id };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _notificationProviderMock.Verify(p => p.SyncSubscriberAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDoNothing_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new CreateNovuSubscriberCommand { UserId = Guid.NewGuid() };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _notificationProviderMock.Verify(p => p.SyncSubscriberAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldLogAndNotThrow_WhenSyncFails()
    {
        // Arrange
        var user = CreateTestUser("auth0|test3", "test3@test.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _notificationProviderMock.Setup(p => p.SyncSubscriberAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                 .ThrowsAsync(new Exception("Novu API failed"));

        var command = new CreateNovuSubscriberCommand { UserId = user.Id };

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.SubscriberId.Should().BeNull(); // Should not be set if sync fails
    }
}
