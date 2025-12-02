
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

public class EventUpdatedEventHandlerTests : TestBase
{
    private readonly Mock<ILogger<EventUpdatedEventHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IN8nService> _n8nServiceMock;
    private readonly EventUpdatedEventHandler _handler;

    public EventUpdatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<EventUpdatedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _currentUserMock = new Mock<ICurrentUser>();
        _n8nServiceMock = new Mock<IN8nService>();
        _handler = new EventUpdatedEventHandler(_loggerMock.Object, _mediatorMock.Object, _currentUserMock.Object, _n8nServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallServicesAndRecordActivity_WhenEventIsUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testEvent = new Event("Test Event", "EVT-TEST", EventType.Other, Guid.NewGuid());
        var notification = new EventUpdatedEvent(testEvent);

        _currentUserMock.Setup(u => u.UserId).Returns(userId);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<RecordActivityCommand>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotCallServices_WhenUserIdIsEmpty()
    {
        // Arrange
        var testEvent = new Event("Test Event", "EVT-TEST", EventType.Other, Guid.NewGuid());
        var notification = new EventUpdatedEvent(testEvent);

        _currentUserMock.Setup(u => u.UserId).Returns(Guid.Empty);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<IRequest>(), CancellationToken.None), Times.Never);
    }
}
