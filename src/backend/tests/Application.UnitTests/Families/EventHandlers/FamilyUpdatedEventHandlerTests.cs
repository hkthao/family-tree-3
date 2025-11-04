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

public class FamilyUpdatedEventHandlerTests : TestBase
{
    private readonly Mock<ILogger<FamilyUpdatedEventHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IGlobalSearchService> _globalSearchServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly FamilyUpdatedEventHandler _handler;

    public FamilyUpdatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<FamilyUpdatedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _globalSearchServiceMock = new Mock<IGlobalSearchService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _handler = new FamilyUpdatedEventHandler(_loggerMock.Object, _mediatorMock.Object, _globalSearchServiceMock.Object, _currentUserMock.Object);
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
        _mediatorMock.Verify(m => m.Send(
            It.Is<RecordActivityCommand>(cmd =>
                cmd.UserId == userId &&
                cmd.ActionType == UserActionType.UpdateFamily &&
                cmd.TargetId == testFamily.Id.ToString()),
            CancellationToken.None), Times.Once);

        _globalSearchServiceMock.Verify(s => s.UpsertEntityAsync(
            testFamily,
            "Family",
            It.IsAny<Func<Family, string>>(),
            It.IsAny<Func<Family, Dictionary<string, string>>>(),
            CancellationToken.None), Times.Once);
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
        _globalSearchServiceMock.Verify(s => s.UpsertEntityAsync(It.IsAny<Family>(), It.IsAny<string>(), It.IsAny<Func<Family, string>>(), It.IsAny<Func<Family, Dictionary<string, string>>>(), CancellationToken.None), Times.Never);
    }
}
