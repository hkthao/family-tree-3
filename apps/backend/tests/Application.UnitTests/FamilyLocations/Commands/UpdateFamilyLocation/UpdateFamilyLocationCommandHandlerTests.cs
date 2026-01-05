using backend.Application.Common.Constants; // Added for ErrorSources
using backend.Application.FamilyLocations.Commands.UpdateFamilyLocation;
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // Added for BaseEvent
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Events;
using FluentAssertions;
using Moq; // Added for Moq.Times
using Xunit;

namespace backend.Application.UnitTests.FamilyLocations.Commands.UpdateFamilyLocation;

public class UpdateFamilyLocationCommandHandlerTests : TestBase
{
    private readonly UpdateFamilyLocationCommandHandler _handler;

    public UpdateFamilyLocationCommandHandlerTests()
    {
        _handler = new UpdateFamilyLocationCommandHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object);
    }

    private Family CreateTestFamily(Guid familyId)
    {
        var family = Family.Create("Test Family", "TF", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        return family;
    }

    private FamilyLocation CreateTestFamilyLocation(Guid familyId, Guid locationId, string name, Guid familyLocationId)
    {
        var location = new Location(name, "Initial Description", 1.0, 1.0, "Initial Address", LocationType.Homeland, LocationAccuracy.Exact, LocationSource.UserSelected);
        SetPrivateProperty(location, "Id", locationId);
        _context.Locations.Add(location); // Explicitly add Location to context
        // Don't call SaveChanges here, let the test method do it after adding FamilyLocation

        var familyLocation = new FamilyLocation(familyId, location.Id);
        SetPrivateProperty(familyLocation, "Location", location);
        SetPrivateProperty(familyLocation, "Id", familyLocationId); // Set a specific Id for FamilyLocation
        return familyLocation;
    }

    [Fact]
    public async Task Handle_ShouldUpdateFamilyLocation_WhenValidCommand()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);

        var locationId = Guid.NewGuid();
        var familyLocationId = Guid.NewGuid(); // Define a specific Id for FamilyLocation
        var existingLocation = CreateTestFamilyLocation(familyId, locationId, "Old Name", familyLocationId);
        await _context.FamilyLocations.AddAsync(existingLocation);
        await _context.SaveChangesAsync();

        var command = new UpdateFamilyLocationCommand
        {
            Id = familyLocationId, // Use the FamilyLocation's actual Id
            FamilyId = familyId,
            LocationId = locationId,
            LocationName = "Updated Name",
            LocationDescription = "Updated Description",
            LocationLatitude = 2.0,
            LocationLongitude = 2.0,
            LocationAddress = "Updated Address",
            LocationType = LocationType.Homeland,
            LocationAccuracy = LocationAccuracy.Approximate,
            LocationSource = LocationSource.Geocoded
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedLocation = await _context.FamilyLocations.FindAsync(familyLocationId);
        updatedLocation.Should().NotBeNull();
        updatedLocation!.Location.Name.Should().Be(command.LocationName);
        updatedLocation.Location.Description.Should().Be(command.LocationDescription);
        updatedLocation.Location.Latitude.Should().Be(command.LocationLatitude);
        updatedLocation.Location.Longitude.Should().Be(command.LocationLongitude);
        updatedLocation.Location.Address.Should().Be(command.LocationAddress);
        updatedLocation.Location.LocationType.Should().Be(command.LocationType);
        updatedLocation.Location.Accuracy.Should().Be(command.LocationAccuracy);
        updatedLocation.Location.Source.Should().Be(command.LocationSource);

        // Verify that the domain event was added
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(
            It.Is<List<BaseEvent>>(events => events.Any(e => e is FamilyLocationUpdatedEvent))
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyLocationNotFound()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var validLocationId = Guid.NewGuid(); // NEW: A valid but non-existent LocationId
        var command = new UpdateFamilyLocationCommand
        {
            Id = Guid.NewGuid(), // Non-existent ID
            FamilyId = familyId,
            LocationId = validLocationId, // NEW: Provide a valid LocationId
            LocationName = "Any Name",
            LocationType = LocationType.Homeland,
            LocationAccuracy = LocationAccuracy.Exact,
            LocationSource = LocationSource.UserSelected
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found.");
        result.ErrorSource.Should().Be(ErrorSources.NotFound);

        // Verify that no domain event was added
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.IsAny<List<BaseEvent>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRaiseFamilyLocationUpdatedEvent()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);

        var locationId = Guid.NewGuid();
        var familyLocationId = Guid.NewGuid(); // Define a specific Id for FamilyLocation
        var existingLocation = CreateTestFamilyLocation(familyId, locationId, "Old Name", familyLocationId);
        await _context.FamilyLocations.AddAsync(existingLocation);
        await _context.SaveChangesAsync();

        var command = new UpdateFamilyLocationCommand
        {
            Id = familyLocationId, // Use the FamilyLocation's actual Id
            FamilyId = familyId,
            LocationId = locationId, // NEW: Add LocationId
            LocationName = "Updated Name",
            LocationType = LocationType.Homeland,
            LocationAccuracy = LocationAccuracy.Approximate,
            LocationSource = LocationSource.Geocoded
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenInvalidCommand()
    {
        // Arrange
        var command = new UpdateFamilyLocationCommand
        {
            Id = Guid.Empty, // Invalid Id
            FamilyId = Guid.Empty, // Invalid FamilyId
            LocationName = "", // Invalid Name
            LocationDescription = "A description",
            LocationLatitude = 10.0,
            LocationLongitude = 20.0,
            LocationAddress = "123 Main St",
            LocationType = LocationType.Homeland,
            LocationAccuracy = LocationAccuracy.Exact,
            LocationSource = LocationSource.UserSelected
        };

        // Act
        var validator = new UpdateFamilyLocationCommandValidator();
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateFamilyLocationCommand.Id));
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateFamilyLocationCommand.FamilyId));
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateFamilyLocationCommand.LocationName));
    }
}

