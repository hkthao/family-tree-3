using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Events.Commands.CreateEvents;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.CreateEvents;

public class CreateEventsCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly CreateEventsCommandHandler _handler;

    public CreateEventsCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _handler = new CreateEventsCommandHandler(_context, _authorizationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateMultipleEventsAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var familyId1 = Guid.NewGuid();
        var familyId2 = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId1, Name = "Family 1", Code = "F1" });
        _context.Families.Add(new Family { Id = familyId2, Name = "Family 2", Code = "F2" });
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId1)).Returns(true);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId2)).Returns(true);

        var eventsToCreate = new List<CreateEventDto>
        {
            new() { Name = "Event 1", FamilyId = familyId1, Type = EventType.Birth, Code = "E1", CalendarType = CalendarType.Solar, SolarDate = DateTime.UtcNow.AddDays(1) },
            new() { Name = "Event 2", FamilyId = familyId2, Type = EventType.Death, Code = "E2", CalendarType = CalendarType.Solar, SolarDate = DateTime.UtcNow.AddDays(2) }
        };
        var command = new CreateEventsCommand(eventsToCreate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdEvents = await _context.Events.ToListAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        createdEvents.Should().HaveCount(2);
        createdEvents.Should().Contain(e => e.Name == "Event 1");
        createdEvents.Should().Contain(e => e.Name == "Event 2");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyIdIsMissing()
    {
        // Arrange
        var eventsToCreate = new List<CreateEventDto>
        {
            new() { Name = "Event with no family id", Type = EventType.Other, Code = "E1", CalendarType = CalendarType.Solar, SolarDate = DateTime.UtcNow, FamilyId = Guid.Empty }
        };
        var command = new CreateEventsCommand(eventsToCreate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.FamilyIdRequired);
        result.ErrorSource.Should().Be(ErrorSources.BadRequest);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorizedForAFamily()
    {
        // Arrange
        var familyId1 = Guid.NewGuid();
        var familyId2 = Guid.NewGuid(); // Unauthorized family
        _context.Families.Add(new Family { Id = familyId1, Name = "Family 1", Code = "F1" });
        _context.Families.Add(new Family { Id = familyId2, Name = "Family 2", Code = "F2" });
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId1)).Returns(true);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId2)).Returns(false); // Not authorized

        var eventsToCreate = new List<CreateEventDto>
        {
            new() { Name = "Event 1", FamilyId = familyId1, Type = EventType.Birth, Code = "E1", CalendarType = CalendarType.Solar, SolarDate = DateTime.UtcNow.AddDays(1) },
            new() { Name = "Event 2", FamilyId = familyId2, Type = EventType.Death, Code = "E2", CalendarType = CalendarType.Solar, SolarDate = DateTime.UtcNow.AddDays(2) }
        };
        var command = new CreateEventsCommand(eventsToCreate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        _context.Events.Should().BeEmpty(); // Ensure no events were created
    }
}
