
using backend.Application.Common.Interfaces;
using backend.Application.Events.EventHandlers;
using backend.Application.UnitTests.Common;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Events.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.EventHandlers;

public class EventDeletedEventHandlerTests : TestBase
{
    private readonly Mock<ILogger<EventDeletedEventHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IDomainEventNotificationPublisher> _notificationPublisherMock;
    private readonly Mock<IGlobalSearchService> _globalSearchServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly EventDeletedEventHandler _handler;

    public EventDeletedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<EventDeletedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _notificationPublisherMock = new Mock<IDomainEventNotificationPublisher>();
        _globalSearchServiceMock = new Mock<IGlobalSearchService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _handler = new EventDeletedEventHandler(_loggerMock.Object, _mediatorMock.Object, _notificationPublisherMock.Object, _globalSearchServiceMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallServicesAndRecordActivity_WhenEventIsDeleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testEvent = new Event("Test Event", "EVT-TEST", EventType.Other, Guid.NewGuid());
        var notification = new EventDeletedEvent(testEvent);

        _currentUserMock.Setup(u => u.UserId).Returns(userId);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.Is<RecordActivityCommand>(cmd => cmd.ActionType == UserActionType.DeleteEvent), CancellationToken.None), Times.Once);
        _notificationPublisherMock.Verify(p => p.PublishNotificationForEventAsync(notification, CancellationToken.None), Times.Once);
        _globalSearchServiceMock.Verify(s => s.DeleteEntityFromSearchAsync(testEvent.Id.ToString(), "Event", CancellationToken.None), Times.Once);
    }
}
