
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
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
    private readonly Mock<IGlobalSearchService> _globalSearchServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IN8nService> _n8nServiceMock;

    public RelationshipCreatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<RelationshipCreatedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _globalSearchServiceMock = new Mock<IGlobalSearchService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _n8nServiceMock = new Mock<IN8nService>();
    }

    [Fact]
    public async Task Handle_ShouldRecordActivity()
    {
        // Arrange
        var handler = new RelationshipCreatedEventHandler(_loggerMock.Object, _mediatorMock.Object, _globalSearchServiceMock.Object, _currentUserMock.Object, _n8nServiceMock.Object);
        var notification = new RelationshipCreatedEvent(new Relationship(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Father));

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(x => x.Send(It.IsAny<RecordActivityCommand>(), CancellationToken.None), Times.Once);
        _n8nServiceMock.Verify(n => n.CallEmbeddingWebhookAsync(It.IsAny<EmbeddingWebhookDto>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPublishNotification()
    {
        // Arrange
        var handler = new RelationshipCreatedEventHandler(_loggerMock.Object, _mediatorMock.Object, _globalSearchServiceMock.Object, _currentUserMock.Object, _n8nServiceMock.Object);
        var notification = new RelationshipCreatedEvent(new Relationship(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Father));

        // Act
        await handler.Handle(notification, CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldUpsertEntityInGlobalSearch()
    {
        // Arrange
        var handler = new RelationshipCreatedEventHandler(_loggerMock.Object, _mediatorMock.Object, _globalSearchServiceMock.Object, _currentUserMock.Object, _n8nServiceMock.Object);
        var notification = new RelationshipCreatedEvent(new Relationship(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Father));

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        _globalSearchServiceMock.Verify(x => x.UpsertEntityAsync(notification.Relationship, "Relationship", It.IsAny<Func<Relationship, string>>(), It.IsAny<Func<Relationship, Dictionary<string, string>>>(), CancellationToken.None), Times.Once);
        _n8nServiceMock.Verify(n => n.CallEmbeddingWebhookAsync(It.IsAny<EmbeddingWebhookDto>(), CancellationToken.None), Times.Once);
    }
}
