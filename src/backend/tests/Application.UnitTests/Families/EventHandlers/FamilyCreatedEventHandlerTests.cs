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

public class FamilyCreatedEventHandlerTests : TestBase
{
    private readonly Mock<ILogger<FamilyCreatedEventHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IGlobalSearchService> _globalSearchServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly FamilyCreatedEventHandler _handler;

    public FamilyCreatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<FamilyCreatedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _globalSearchServiceMock = new Mock<IGlobalSearchService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _handler = new FamilyCreatedEventHandler(_loggerMock.Object, _mediatorMock.Object, _globalSearchServiceMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallServicesAndRecordActivity_WhenFamilyIsCreated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testFamily = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF1", Description = "A test family", Address = "Test Address" };
        var notification = new FamilyCreatedEvent(testFamily);

        _currentUserMock.Setup(u => u.UserId).Returns(userId);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert

        // Verify that RecordActivityCommand was sent
        _mediatorMock.Verify(m => m.Send(
            It.Is<RecordActivityCommand>(cmd =>
                cmd.UserId == userId &&
                cmd.ActionType == UserActionType.CreateFamily &&
                cmd.TargetId == testFamily.Id.ToString()),
            CancellationToken.None), Times.Once);

        // Verify that entity was upserted to global search
        _globalSearchServiceMock.Verify(s => s.UpsertEntityAsync(
            testFamily,
            "Family",
            It.IsAny<Func<Family, string>>(),
            It.IsAny<Func<Family, Dictionary<string, string>>>(),
            CancellationToken.None), Times.Once);
    }
}
