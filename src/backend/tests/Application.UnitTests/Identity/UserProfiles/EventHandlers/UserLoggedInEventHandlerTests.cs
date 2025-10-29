using System.Security.Claims;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Identity.UserProfiles.Commands.SyncNotificationSubscriber;
using backend.Application.Identity.UserProfiles.Commands.SyncUserProfile;
using backend.Application.Identity.UserProfiles.EventHandlers;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Application.UnitTests.Common;
using backend.Domain.Events;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.EventHandlers;

public class UserLoggedInEventHandlerTests : TestBase
{
    private readonly Mock<ILogger<UserLoggedInEventHandler>> _mockLogger;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly UserLoggedInEventHandler _handler;

    public UserLoggedInEventHandlerTests()
    {
        _mockLogger = new Mock<ILogger<UserLoggedInEventHandler>>();
        _mockMediator = new Mock<IMediator>();
        _mockNotificationService = new Mock<INotificationService>();

        _handler = new UserLoggedInEventHandler(
            _mockLogger.Object,
            _mockMediator.Object,
            _mockNotificationService.Object
        );
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng thông báo chào mừng được gửi cho người dùng mới.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập một ClaimsPrincipal hợp lệ. Mock mediator để trả về UserProfileDto với IsNewUser = true
    ///               cho SyncUserProfileCommand, và trả về thành công cho SyncNotificationSubscriberCommand.
    ///    - Act: Gửi UserLoggedInEvent.
    ///    - Assert: Xác minh rằng SendNotificationAsync của INotificationService được gọi một lần với thông báo chào mừng chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldSendWelcomeNotificationForNewUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, "newuser@example.com"),
            new(ClaimTypes.Name, "New User"),
            new(ClaimTypes.GivenName, "New"),
            new(ClaimTypes.Surname, "User")
        }));

        var userProfileDto = new UserProfileDto
        {
            Id = userId,
            ExternalId = userId.ToString(),
            Email = "newuser@example.com",
            Name = "New User",
            FirstName = "New",
            LastName = "User",
            IsNewUser = true
        };

        _mockMediator.Setup(m => m.Send(It.IsAny<SyncUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserProfileDto>.Success(userProfileDto));

        _mockMediator.Setup(m => m.Send(It.IsAny<SyncNotificationSubscriberCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var notification = new UserLoggedInEvent(userPrincipal);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mockNotificationService.Verify(s => s.SendNotificationAsync(
            It.Is<NotificationMessage>(msg =>
                msg.RecipientUserId == userId.ToString() &&
                msg.Title == NovuWorkflow.WelcomeNotification &&
                msg.Metadata != null &&
                msg.Metadata.ContainsKey("firstName") &&
                string.Equals(msg.Metadata["firstName"], userProfileDto.FirstName)),
            It.IsAny<CancellationToken>()),
            Times.Once);
        _mockLogger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng thông báo chào mừng không được gửi cho người dùng hiện có.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập một ClaimsPrincipal hợp lệ. Mock mediator để trả về UserProfileDto với IsNewUser = false
    ///               cho SyncUserProfileCommand, và trả về thành công cho SyncNotificationSubscriberCommand.
    ///    - Act: Gửi UserLoggedInEvent.
    ///    - Assert: Xác minh rằng SendNotificationAsync của INotificationService không bao giờ được gọi.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldNotSendWelcomeNotificationForExistingUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, "existinguser@example.com"),
            new(ClaimTypes.Name, "Existing User")
        }));

        var userProfileDto = new UserProfileDto
        {
            Id = userId,
            ExternalId = userId.ToString(),
            Email = "existinguser@example.com",
            Name = "Existing User",
            IsNewUser = false // Existing user
        };

        _mockMediator.Setup(m => m.Send(It.IsAny<SyncUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserProfileDto>.Success(userProfileDto));

        _mockMediator.Setup(m => m.Send(It.IsAny<SyncNotificationSubscriberCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var notification = new UserLoggedInEvent(userPrincipal);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mockNotificationService.Verify(s => s.SendNotificationAsync(
            It.IsAny<NotificationMessage>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
        _mockLogger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once); // Log for SyncUserProfile
    }
}
