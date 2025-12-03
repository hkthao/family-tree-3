using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Events.Commands.CreateEvent;
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // NEW
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore; // NEW
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.CreateEvent;

public class CreateEventCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly CreateEventCommandHandler _handler;

    public CreateEventCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _handler = new CreateEventCommandHandler(_context, _authorizationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateEventAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF1" });
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new CreateEventCommand
        {
            Name = "New Year Party",
            FamilyId = familyId,
            Type = EventType.Other,
            StartDate = new DateTime(2025, 1, 1)
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdEvent = await _context.Events.FindAsync(result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdEvent.Should().NotBeNull();
        createdEvent!.Name.Should().Be(command.Name);
        createdEvent.FamilyId.Should().Be(command.FamilyId);
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is Domain.Events.Events.EventCreatedEvent)
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new CreateEventCommand { FamilyId = familyId, Name = "Unauthorized Event" };

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldGenerateCode_WhenCodeIsNotProvided()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF1" });
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new CreateEventCommand
        {
            Name = "Event without code",
            FamilyId = familyId,
            Type = EventType.Other
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdEvent = await _context.Events.FindAsync(result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdEvent.Should().NotBeNull();
        createdEvent!.Code.Should().NotBeNullOrEmpty();
        createdEvent.Code.Should().StartWith("EVT-");
    }

    [Fact]
    public async Task Handle_ShouldAddRelatedMembers_WhenProvided()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var member1Id = Guid.NewGuid();
        var member2Id = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF1" });
        _context.Members.Add(new Member("first", "last", "c1", familyId) { Id = member1Id });
        _context.Members.Add(new Member("first2", "last2", "c2", familyId) { Id = member2Id });
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new CreateEventCommand
        {
            Name = "Event with members",
            FamilyId = familyId,
            Type = EventType.Other,
            RelatedMembers = new List<Guid> { member1Id, member2Id }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdEvent = await _context.Events.Include(e => e.EventMembers).FirstOrDefaultAsync(e => e.Id == result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdEvent.Should().NotBeNull();
        createdEvent!.EventMembers.Should().HaveCount(2);
        createdEvent.EventMembers.Select(em => em.MemberId).Should().Contain(member1Id);
        createdEvent.EventMembers.Select(em => em.MemberId).Should().Contain(member2Id);
    }
}
