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

    private FamilyLocation CreateTestFamilyLocation(Guid familyId, Guid locationId, string name)
    {
        return new FamilyLocation
        {
            Id = locationId,
            FamilyId = familyId,
            Name = name,
            Description = "Initial Description",
            Latitude = 1.0,
            Longitude = 1.0,
            Address = "Initial Address",
            LocationType = LocationType.Homeland,
            Accuracy = LocationAccuracy.Exact,
            Source = LocationSource.UserSelected
        };
    }

    [Fact]
    public async Task Handle_ShouldUpdateFamilyLocation_WhenValidCommand()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);

        var locationId = Guid.NewGuid();
        var existingLocation = CreateTestFamilyLocation(familyId, locationId, "Old Name");
        await _context.FamilyLocations.AddAsync(existingLocation);
        await _context.SaveChangesAsync();

        var command = new UpdateFamilyLocationCommand
        {
            Id = locationId,
            FamilyId = familyId,
            Name = "Updated Name",
            Description = "Updated Description",
            Latitude = 2.0,
            Longitude = 2.0,
            Address = "Updated Address",
            LocationType = LocationType.Homeland, // Using Homeland as closest
            Accuracy = LocationAccuracy.Approximate,
            Source = LocationSource.Geocoded
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedLocation = await _context.FamilyLocations.FindAsync(locationId);
        updatedLocation.Should().NotBeNull();
        updatedLocation!.Name.Should().Be(command.Name);
        updatedLocation.Description.Should().Be(command.Description);
        updatedLocation.Latitude.Should().Be(command.Latitude);
        updatedLocation.Longitude.Should().Be(command.Longitude);
        updatedLocation.Address.Should().Be(command.Address);
        updatedLocation.LocationType.Should().Be(command.LocationType);
        updatedLocation.Accuracy.Should().Be(command.Accuracy);
        updatedLocation.Source.Should().Be(command.Source);

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
        var command = new UpdateFamilyLocationCommand
        {
            Id = Guid.NewGuid(), // Non-existent ID
            FamilyId = familyId,
            Name = "Any Name",
            LocationType = LocationType.Homeland,
            Accuracy = LocationAccuracy.Exact,
            Source = LocationSource.UserSelected
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
        var existingLocation = CreateTestFamilyLocation(familyId, locationId, "Old Name");
        await _context.FamilyLocations.AddAsync(existingLocation);
        await _context.SaveChangesAsync();

        var command = new UpdateFamilyLocationCommand
        {
            Id = locationId,
            FamilyId = familyId,
            Name = "Updated Name",
            LocationType = LocationType.Homeland, // Using Homeland as closest
            Accuracy = LocationAccuracy.Approximate,
            Source = LocationSource.Geocoded
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(
            It.Is<List<BaseEvent>>(events => events.Any(e => e is FamilyLocationUpdatedEvent))
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenInvalidCommand()
    {
        // Arrange
        var command = new UpdateFamilyLocationCommand
        {
            Id = Guid.Empty, // Invalid Id
            FamilyId = Guid.Empty, // Invalid FamilyId
            Name = "", // Invalid Name
            Description = "A description",
            Latitude = 10.0,
            Longitude = 20.0,
            Address = "123 Main St",
            LocationType = LocationType.Homeland,
            Accuracy = LocationAccuracy.Exact,
            Source = LocationSource.UserSelected
        };

        // Act
        var validator = new UpdateFamilyLocationCommandValidator();
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateFamilyLocationCommand.Id));
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateFamilyLocationCommand.FamilyId));
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateFamilyLocationCommand.Name));
    }
}
