
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

public class EventCreatedEventHandlerTests : TestBase
{
    private readonly Mock<ILogger<EventCreatedEventHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IDomainEventNotificationPublisher> _notificationPublisherMock;
    private readonly Mock<IGlobalSearchService> _globalSearchServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly EventCreatedEventHandler _handler;

    public EventCreatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<EventCreatedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _notificationPublisherMock = new Mock<IDomainEventNotificationPublisher>();
        _globalSearchServiceMock = new Mock<IGlobalSearchService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _handler = new EventCreatedEventHandler(_loggerMock.Object, _mediatorMock.Object, _notificationPublisherMock.Object, _globalSearchServiceMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallServicesAndRecordActivity_WhenEventIsCreated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testEvent = new Event("Test Event", "EVT-TEST", EventType.Other, Guid.NewGuid());
        var notification = new EventCreatedEvent(testEvent);

        _currentUserMock.Setup(u => u.UserId).Returns(userId);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert

        // Verify that RecordActivityCommand was sent
        _mediatorMock.Verify(m => m.Send(
            It.Is<RecordActivityCommand>(cmd => 
                cmd.UserId == userId &&
                cmd.ActionType == UserActionType.CreateEvent &&
                cmd.TargetId == testEvent.Id.ToString()), 
            CancellationToken.None), Times.Once);

        // Verify that notification was published
        _notificationPublisherMock.Verify(p => p.PublishNotificationForEventAsync(notification, CancellationToken.None), Times.Once);

        // Verify that entity was upserted to global search
        _globalSearchServiceMock.Verify(s => s.UpsertEntityAsync(
            testEvent,
            "Event",
            It.IsAny<Func<Event, string>>(),
            It.IsAny<Func<Event, Dictionary<string, string>>>(),
            CancellationToken.None), Times.Once);
    }
}
