
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

public class MemberDeletedEventHandlerTests
{
    private readonly Mock<ILogger<MemberDeletedEventHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IGlobalSearchService> _globalSearchServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly MemberDeletedEventHandler _handler;

    public MemberDeletedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<MemberDeletedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _globalSearchServiceMock = new Mock<IGlobalSearchService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _handler = new MemberDeletedEventHandler(_loggerMock.Object, _mediatorMock.Object, _globalSearchServiceMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallServicesAndRecordActivity()
    {
        // Arrange
        var member = new Member("John", "Doe", "JD", Guid.NewGuid());
        var notification = new MemberDeletedEvent(member);
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(u => u.UserId).Returns(userId);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.Is<RecordActivityCommand>(cmd => cmd.ActionType == UserActionType.DeleteMember), CancellationToken.None), Times.Once);
        _globalSearchServiceMock.Verify(s => s.DeleteEntityFromSearchAsync(member.Id.ToString(), "Member", CancellationToken.None), Times.Once);
    }
}
