using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.EventHandlers;
using backend.Application.UnitTests.Common;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Events.Families;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.EventHandlers;

public class FamilyUpdatedEventHandlerTests : TestBase
{
    private readonly Mock<ILogger<FamilyUpdatedEventHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IN8nService> _n8nServiceMock;
    private readonly FamilyUpdatedEventHandler _handler;

    public FamilyUpdatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<FamilyUpdatedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _currentUserMock = new Mock<ICurrentUser>();
        _n8nServiceMock = new Mock<IN8nService>();
        _handler = new FamilyUpdatedEventHandler(_loggerMock.Object, _mediatorMock.Object, _currentUserMock.Object, _n8nServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallServicesAndRecordActivity_WhenFamilyIsUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testFamily = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF1", Description = "A test family", Address = "Test Address" };
        var notification = new FamilyUpdatedEvent(testFamily);

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
        var testFamily = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF1" };
        var notification = new FamilyUpdatedEvent(testFamily);

        _currentUserMock.Setup(u => u.UserId).Returns(Guid.Empty);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<IRequest>(), CancellationToken.None), Times.Never);
    }
}
