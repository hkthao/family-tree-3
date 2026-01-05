using backend.Application.Common.Constants; // Added for ErrorSources
using backend.Application.FamilyLocations.Commands.DeleteFamilyLocation;
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // Added for BaseEvent
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Events;
using backend.Infrastructure.Data; // Added for ApplicationDbContext
using FluentAssertions;
using Moq; // Added for Moq.Times
using Xunit;

namespace backend.Application.UnitTests.FamilyLocations.Commands.DeleteFamilyLocation;

public class DeleteFamilyLocationCommandHandlerTests : TestBase
{
    private readonly DeleteFamilyLocationCommandHandler _handler;

    public DeleteFamilyLocationCommandHandlerTests()
    {
        _handler = new DeleteFamilyLocationCommandHandler(_context, _mockUser.Object, _mockAuthorizationService.Object);
    }

    private Family CreateTestFamily(Guid familyId)
    {
        var family = Family.Create("Test Family", "TF", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        return family;
    }

    private FamilyLocation CreateTestFamilyLocation(Guid familyId, Guid locationId, string name, Guid familyLocationId)
    {
        var location = new Location(name, "Test Description", 1.0, 1.0, "Test Address", LocationType.Homeland, LocationAccuracy.Exact, LocationSource.UserSelected);
        var familyLocation = new FamilyLocation(familyId, locationId);
        SetPrivateProperty(familyLocation, "Location", location);
        SetPrivateProperty(familyLocation, "Id", familyLocationId); // Set the FamilyLocation's Id
        return familyLocation;
    }

    [Fact]
    public async Task Handle_ShouldDeleteFamilyLocation_WhenValidCommand()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);

        var locationId = Guid.NewGuid();
        var familyLocationId = Guid.NewGuid(); // Define a specific Id for FamilyLocation
        var existingLocation = CreateTestFamilyLocation(familyId, locationId, "Location to Delete", familyLocationId);
        await _context.FamilyLocations.AddAsync(existingLocation);
        await _context.SaveChangesAsync();

        var command = new DeleteFamilyLocationCommand(familyLocationId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Use a fresh context to verify the deletion
        await using var verificationContext = new ApplicationDbContext(_dbContextOptions, _mockDomainEventDispatcher.Object, _mockUser.Object, _mockDateTime.Object);
        var deletedLocation = await verificationContext.FamilyLocations.FindAsync(locationId);
        deletedLocation.Should().BeNull();

        // Verify that the domain event was added
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(
            It.Is<List<BaseEvent>>(events => events.Any(e => e is FamilyLocationDeletedEvent))
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyLocationNotFound()
    {
        // Arrange
        var command = new DeleteFamilyLocationCommand(Guid.NewGuid()); // Non-existent ID
        var handler = new DeleteFamilyLocationCommandHandler(_context, _mockUser.Object, _mockAuthorizationService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found.");
        result.ErrorSource.Should().Be(ErrorSources.NotFound);

        // Verify that no domain event was added
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.IsAny<List<BaseEvent>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRaiseFamilyLocationDeletedEvent()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);

        var locationId = Guid.NewGuid();
        var familyLocationId = Guid.NewGuid(); // Define a specific Id for FamilyLocation
        var existingLocation = CreateTestFamilyLocation(familyId, locationId, "Location to Delete", familyLocationId);
        await _context.FamilyLocations.AddAsync(existingLocation);
        await _context.SaveChangesAsync();

        var command = new DeleteFamilyLocationCommand(familyLocationId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(
            It.Is<List<BaseEvent>>(events => events.Any(e => e is FamilyLocationDeletedEvent))
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenInvalidCommand()
    {
        // Arrange
        var command = new DeleteFamilyLocationCommand(Guid.Empty); // Invalid Id

        // Act
        var validator = new DeleteFamilyLocationCommandValidator();
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteFamilyLocationCommand.Id));
    }
}
