
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

public class MemberUpdatedEventHandlerTests
{
    private readonly Mock<ILogger<MemberUpdatedEventHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IDomainEventNotificationPublisher> _notificationPublisherMock;
    private readonly Mock<IGlobalSearchService> _globalSearchServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly MemberUpdatedEventHandler _handler;

    public MemberUpdatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<MemberUpdatedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _notificationPublisherMock = new Mock<IDomainEventNotificationPublisher>();
        _globalSearchServiceMock = new Mock<IGlobalSearchService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _handler = new MemberUpdatedEventHandler(_loggerMock.Object, _mediatorMock.Object, _notificationPublisherMock.Object, _globalSearchServiceMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallServicesAndRecordActivity()
    {
        // Arrange
        var member = new Member("John", "Doe", "JD", Guid.NewGuid());
        var notification = new MemberUpdatedEvent(member);
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(u => u.ProfileId).Returns(userId);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.Is<RecordActivityCommand>(cmd => cmd.ActionType == UserActionType.UpdateMember), CancellationToken.None), Times.Once);
        _notificationPublisherMock.Verify(p => p.PublishNotificationForEventAsync(notification, CancellationToken.None), Times.Once);
        _globalSearchServiceMock.Verify(s => s.UpsertEntityAsync(member, "Member", It.IsAny<Func<Member, string>>(), It.IsAny<Func<Member, Dictionary<string, string>>>(), CancellationToken.None), Times.Once);
    }
}
