
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
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IN8nService> _n8nServiceMock;
    private readonly EventCreatedEventHandler _handler;

    public EventCreatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<EventCreatedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _currentUserMock = new Mock<ICurrentUser>();
        _n8nServiceMock = new Mock<IN8nService>();
        _handler = new EventCreatedEventHandler(_loggerMock.Object, _mediatorMock.Object, _currentUserMock.Object, _n8nServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallServicesAndRecordActivity_WhenEventIsCreated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testEvent = Event.CreateSolarEvent(
            name: "Test Event",
            code: "EVT-TEST",
            type: EventType.Other,
            solarDate: DateTime.UtcNow, // Dummy value
            repeatRule: RepeatRule.None, // Dummy value
            familyId: Guid.NewGuid()
        );
        var notification = new EventCreatedEvent(testEvent);

        _currentUserMock.Setup(u => u.UserId).Returns(userId);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert

        _mediatorMock.Verify(m => m.Send(It.IsAny<RecordActivityCommand>(), CancellationToken.None), Times.Once);
    }
}
