using backend.Application.Common.Constants;
using backend.Application.Events.Queries.GetEventById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Events.Queries.GetEventById;

public class GetEventByIdQueryHandlerTests : TestBase
{
    public GetEventByIdQueryHandlerTests()
    {
        // TestBase already sets up _mockUser and _mockAuthorizationService
        // Set default authenticated user for specific scenarios if needed
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockUser.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true); // Default to admin for these tests to easily access events
    }

    [Fact]
    public async Task Handle_ShouldReturnEvent_WhenEventExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var testEvent = Event.CreateSolarEvent("Test Event", "EVT-TEST", EventType.Other, DateTime.UtcNow.AddDays(-10), RepeatRule.None, familyId);
        testEvent.Id = Guid.NewGuid(); // Ensure Id is set for entity tracking
        _context.Events.Add(testEvent);
        await _context.SaveChangesAsync();

        var query = new GetEventByIdQuery(testEvent.Id);
        var handler = new GetEventByIdQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(testEvent.Id);
        result.Value.Name.Should().Be(testEvent.Name);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEventDoesNotExist()
    {
        // Arrange
        var nonExistentEventId = Guid.NewGuid();
        var query = new GetEventByIdQuery(nonExistentEventId);
        var handler = new GetEventByIdQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.EventNotFound, nonExistentEventId)); // Corrected assertion
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        _mockUser.Setup(x => x.IsAuthenticated).Returns(false);
        _mockUser.Setup(x => x.UserId).Returns(Guid.Empty);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);

        var eventId = Guid.NewGuid();
        var handler = new GetEventByIdQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);
        var query = new GetEventByIdQuery(eventId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.Unauthorized); // Corrected assertion
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenNonAdminUserHasNoAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var inaccessibleFamilyId = Guid.NewGuid();

        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false); // Non-admin user

        // Create an accessible family and associate the user with it
        var accessibleFamily = new Family { Id = Guid.NewGuid(), Name = "Accessible Family", Code = "ACC", Visibility = "Private" };
        accessibleFamily.AddFamilyUser(userId, FamilyRole.Viewer);
        _context.Families.Add(accessibleFamily);
        await _context.SaveChangesAsync();


        // Create an event in an inaccessible family
        var inaccessibleEvent = Event.CreateSolarEvent("Inaccessible Event", "EVT-INACCESSIBLE", EventType.Other, DateTime.UtcNow.AddDays(5), RepeatRule.None, inaccessibleFamilyId);
        inaccessibleEvent.Id = Guid.NewGuid();
        _context.Events.Add(inaccessibleEvent);

        await _context.SaveChangesAsync();

        var handler = new GetEventByIdQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);
        var query = new GetEventByIdQuery(inaccessibleEvent.Id); // Try to get the inaccessible event

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.EventNotFound, inaccessibleEvent.Id)); // Corrected assertion
    }
}
