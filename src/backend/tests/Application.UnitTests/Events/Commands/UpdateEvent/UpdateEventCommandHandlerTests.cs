
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Events.Commands.UpdateEvent;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly UpdateEventCommandHandler _handler;

    public UpdateEventCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _handler = new UpdateEventCommandHandler(_context, _authorizationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateEventAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var existingEvent = new Event("Old Name", "EVT-OLD", EventType.Other, familyId) { Id = eventId };
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateEventCommand
        {
            Id = eventId,
            Name = "New Name",
            Description = "New Description",
            FamilyId = familyId,
            Type = EventType.Birth
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedEvent = await _context.Events.FindAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedEvent.Should().NotBeNull();
        updatedEvent!.Name.Should().Be(command.Name);
        updatedEvent.Description.Should().Be(command.Description);
        updatedEvent.Type.Should().Be(command.Type);
        updatedEvent.DomainEvents.Should().ContainSingle(e => e is Domain.Events.Events.EventUpdatedEvent);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEventNotFound()
    {
        // Arrange
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), FamilyId = Guid.NewGuid() };
        _authorizationServiceMock.Setup(x => x.CanManageFamily(It.IsAny<Guid>())).Returns(true);

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
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), FamilyId = familyId };

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldAddAndRemoveMembers_WhenUpdatingRelatedMembers()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var member1Id = Guid.NewGuid();
        var member2Id = Guid.NewGuid();
        var member3Id = Guid.NewGuid();

        var existingEvent = new Event("Test Event", "EVT-TEST", EventType.Other, familyId) { Id = eventId };
        existingEvent.AddEventMember(member1Id);
        _context.Events.Add(existingEvent);
        _context.Members.Add(new Member("first", "last", "c1", familyId) { Id = member2Id });
        _context.Members.Add(new Member("first2", "last2", "c2", familyId) { Id = member3Id });
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateEventCommand
        {
            Id = eventId,
            FamilyId = familyId,
            Name = "Updated Event",
            RelatedMembers = new List<Guid> { member2Id, member3Id }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedEvent = await _context.Events.Include(e => e.EventMembers).FirstOrDefaultAsync(e => e.Id == eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedEvent.Should().NotBeNull();
        updatedEvent!.EventMembers.Should().HaveCount(2);
        updatedEvent.EventMembers.Select(em => em.MemberId).Should().NotContain(member1Id);
        updatedEvent.EventMembers.Select(em => em.MemberId).Should().Contain(member2Id);
        updatedEvent.EventMembers.Select(em => em.MemberId).Should().Contain(member3Id);
    }
}
