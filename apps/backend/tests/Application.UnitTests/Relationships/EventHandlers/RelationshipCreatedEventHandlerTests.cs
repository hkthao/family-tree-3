
using backend.Application.Common.Interfaces;
using backend.Application.Relationships.EventHandlers;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Events.Relationships;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Relationships.EventHandlers;

public class RelationshipCreatedEventHandlerTests
{
    private readonly Mock<ILogger<RelationshipCreatedEventHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ICurrentUser> _currentUserMock;

    public RelationshipCreatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<RelationshipCreatedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _currentUserMock = new Mock<ICurrentUser>();
    }

    [Fact]
    public async Task Handle_ShouldRecordActivity()
    {
        // Arrange
        var handler = new RelationshipCreatedEventHandler(_loggerMock.Object, _mediatorMock.Object, _currentUserMock.Object);
        var notification = new RelationshipCreatedEvent(new Relationship(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Father));

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(x => x.Send(It.IsAny<RecordActivityCommand>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPublishNotification()
    {
        // Arrange
        var handler = new RelationshipCreatedEventHandler(_loggerMock.Object, _mediatorMock.Object, _currentUserMock.Object);
        var notification = new RelationshipCreatedEvent(new Relationship(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Father));

        // Act
        await handler.Handle(notification, CancellationToken.None);
    }
}
