
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Events.Commands.DeleteEvent;
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // NEW
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.DeleteEvent;

public class DeleteEventHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly DeleteEventCommandHandler _handler;

    public DeleteEventHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _dateTimeMock = new Mock<IDateTime>();
        _handler = new DeleteEventCommandHandler(_context, _authorizationServiceMock.Object, _currentUserMock.Object, _dateTimeMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteEventAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var existingEvent = new Event("Test Event", "EVT-TEST", EventType.Other, familyId) { Id = eventId };
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _dateTimeMock.Setup(x => x.Now).Returns(now);

        var command = new DeleteEventCommand(eventId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var deletedEvent = await _context.Events.FindAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        deletedEvent.Should().NotBeNull();
        deletedEvent!.IsDeleted.Should().BeTrue();
        deletedEvent.DeletedBy.Should().Be(userId.ToString());
        deletedEvent.DeletedDate.Should().Be(now);
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is Domain.Events.Events.EventDeletedEvent)
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEventNotFound()
    {
        // Arrange
        var command = new DeleteEventCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.EventNotFound, command.Id));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var existingEvent = new Event("Test Event", "EVT-TEST", EventType.Other, familyId) { Id = eventId };
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false);

        var command = new DeleteEventCommand(eventId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }
}
