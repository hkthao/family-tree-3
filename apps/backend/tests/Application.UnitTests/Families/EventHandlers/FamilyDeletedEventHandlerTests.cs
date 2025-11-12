using backend.Application.Common.Interfaces;
using backend.Application.Families.EventHandlers;
using backend.Application.UnitTests.Common;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Events.Families;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.EventHandlers;

public class FamilyDeletedEventHandlerTests : TestBase
{
    private readonly Mock<ILogger<FamilyDeletedEventHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly FamilyDeletedEventHandler _handler;

    public FamilyDeletedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<FamilyDeletedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _currentUserMock = new Mock<ICurrentUser>();
        _handler = new FamilyDeletedEventHandler(_loggerMock.Object, _mediatorMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallServicesAndRecordActivity_WhenFamilyIsDeleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testFamily = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF1" };
        var notification = new FamilyDeletedEvent(testFamily);

        _currentUserMock.Setup(u => u.UserId).Returns(userId);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<RecordActivityCommand>(), CancellationToken.None), Times.Once);
    }
}
