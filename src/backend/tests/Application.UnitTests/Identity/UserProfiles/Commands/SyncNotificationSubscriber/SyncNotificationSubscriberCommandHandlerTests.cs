using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Identity.UserProfiles.Commands.SyncNotificationSubscriber;
using backend.Application.Identity.UserProfiles.Queries;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Commands.SyncNotificationSubscriber;

public class SyncNotificationSubscriberCommandHandlerTests
{
    private readonly Mock<ILogger<SyncNotificationSubscriberCommandHandler>> _loggerMock;
    private readonly Mock<INotificationProviderFactory> _notificationProviderFactoryMock;
    private readonly Mock<INotificationProvider> _notificationProviderMock;
    private readonly Mock<IOptions<NotificationSettings>> _notificationSettingsMock;

    public SyncNotificationSubscriberCommandHandlerTests()
    {
        _loggerMock = new Mock<ILogger<SyncNotificationSubscriberCommandHandler>>();
        _notificationProviderFactoryMock = new Mock<INotificationProviderFactory>();
        _notificationProviderMock = new Mock<INotificationProvider>();
        _notificationSettingsMock = new Mock<IOptions<NotificationSettings>>();
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenNotificationProviderIsNotConfigured()
    {
        // Arrange
        _notificationSettingsMock.Setup(x => x.Value).Returns(new NotificationSettings { Provider = null! });
        var _handler = new SyncNotificationSubscriberCommandHandler(
             _loggerMock.Object,
             _notificationProviderFactoryMock.Object,
             _notificationSettingsMock.Object);
        var command = new SyncNotificationSubscriberCommand
        {
            UserProfile = new UserProfileDto { Id = Guid.NewGuid(), Email = "test@example.com" }
        };
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();

        _notificationProviderFactoryMock.Verify(x => x.GetProvider(It.IsAny<string>()), Times.Never);
        _notificationProviderMock.Verify(x => x.SyncSubscriberAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_OnSuccessfulSynchronization()
    {
        // Arrange
        _notificationSettingsMock.Setup(x => x.Value).Returns(new NotificationSettings { Provider = "Novu" });
        _notificationProviderFactoryMock.Setup(x => x.GetProvider("Novu"))
            .Returns(_notificationProviderMock.Object);
        var userProfile = new UserProfileDto
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Phone = "1234567890"
        };
        var command = new SyncNotificationSubscriberCommand { UserProfile = userProfile };

        _notificationProviderMock.Setup(x => x.SyncSubscriberAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var _handler = new SyncNotificationSubscriberCommandHandler(
                    _loggerMock.Object,
                    _notificationProviderFactoryMock.Object,
                    _notificationSettingsMock.Object);
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _notificationProviderFactoryMock.Verify(x => x.GetProvider("Novu"), Times.Once);
        _notificationProviderMock.Verify(x => x.SyncSubscriberAsync(
            userProfile.Id.ToString(),
            userProfile.FirstName,
            userProfile.LastName,
            userProfile.Email,
            userProfile.Phone),
            Times.Once);

    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_OnSynchronizationException()
    {
        // Arrange
        _notificationSettingsMock.Setup(x => x.Value).Returns(new NotificationSettings { Provider = "Novu" });
        _notificationProviderFactoryMock.Setup(x => x.GetProvider("Novu"))
            .Returns(_notificationProviderMock.Object);
        var userProfile = new UserProfileDto
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com"
        };
        var command = new SyncNotificationSubscriberCommand { UserProfile = userProfile };
        var exceptionMessage = "Network error";

        _notificationProviderMock.Setup(x => x.SyncSubscriberAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var _handler = new SyncNotificationSubscriberCommandHandler(
                    _loggerMock.Object,
                    _notificationProviderFactoryMock.Object,
                    _notificationSettingsMock.Object);
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.UnexpectedError, exceptionMessage));
        result.ErrorSource.Should().Be(ErrorSources.Exception);

    }
}
