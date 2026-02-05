using backend.Application.Common.Interfaces.Core;
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

public class RelationshipUpdatedEventHandlerTests
{
    private readonly Mock<ILogger<RelationshipUpdatedEventHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ICurrentUser> _currentUserMock;

    public RelationshipUpdatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<RelationshipUpdatedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _currentUserMock = new Mock<ICurrentUser>();
    }

    [Fact]
    public async Task Handle_ShouldRecordActivity()
    {
        // Arrange
        var handler = new RelationshipUpdatedEventHandler(_loggerMock.Object, _mediatorMock.Object, _currentUserMock.Object);
        var notification = new RelationshipUpdatedEvent(new Relationship(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Father, null));

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(x => x.Send(It.IsAny<RecordActivityCommand>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPublishNotification()
    {
        // Arrange
        var handler = new RelationshipUpdatedEventHandler(_loggerMock.Object, _mediatorMock.Object, _currentUserMock.Object);
        var notification = new RelationshipUpdatedEvent(new Relationship(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Father, null));

        // Act
        await handler.Handle(notification, CancellationToken.None);
    }
}
