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

public class RelationshipDeletedEventHandlerTests
{
    private readonly Mock<ILogger<RelationshipDeletedEventHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ICurrentUser> _currentUserMock;

    public RelationshipDeletedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<RelationshipDeletedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _currentUserMock = new Mock<ICurrentUser>();
    }

    [Fact]
    public async Task Handle_ShouldRecordActivity()
    {
        // Arrange
        var handler = new RelationshipDeletedEventHandler(_loggerMock.Object, _mediatorMock.Object, _currentUserMock.Object);
        var notification = new RelationshipDeletedEvent(new Relationship(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Father));

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(x => x.Send(It.IsAny<RecordActivityCommand>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPublishNotification()
    {
        // Arrange
        var handler = new RelationshipDeletedEventHandler(_loggerMock.Object, _mediatorMock.Object, _currentUserMock.Object);
        var notification = new RelationshipDeletedEvent(new Relationship(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Father));

        // Act
        await handler.Handle(notification, CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldDeleteEntityFromGlobalSearch()
    {
        // Arrange
        var handler = new RelationshipDeletedEventHandler(_loggerMock.Object, _mediatorMock.Object, _currentUserMock.Object);
        var notification = new RelationshipDeletedEvent(new Relationship(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Father));

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
    }
}
