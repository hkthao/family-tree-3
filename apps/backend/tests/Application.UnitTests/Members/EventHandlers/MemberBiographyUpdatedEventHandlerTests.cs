
using backend.Application.Common.Interfaces;
using backend.Application.Members.EventHandlers;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Events.Members;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.EventHandlers;

public class MemberBiographyUpdatedEventHandlerTests
{
    private readonly Mock<ILogger<MemberBiographyUpdatedEventHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly MemberBiographyUpdatedEventHandler _handler;

    public MemberBiographyUpdatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<MemberBiographyUpdatedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _currentUserMock = new Mock<ICurrentUser>();
        _handler = new MemberBiographyUpdatedEventHandler(_loggerMock.Object, _mediatorMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallServicesAndRecordActivity()
    {
        // Arrange
        var member = new Member("John", "Doe", "JD", Guid.NewGuid());
        var notification = new MemberBiographyUpdatedEvent(member);
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(u => u.UserId).Returns(userId);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.Is<RecordActivityCommand>(cmd => cmd.ActionType == UserActionType.UpdateMember), CancellationToken.None), Times.Once);
    }
}
