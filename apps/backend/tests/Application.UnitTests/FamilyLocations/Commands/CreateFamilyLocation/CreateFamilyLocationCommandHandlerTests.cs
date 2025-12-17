using backend.Application.Common.Models;
using backend.Application.FamilyLocations.Commands.CreateFamilyLocation;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Events;
using FluentAssertions;
using Xunit;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq; // Added for Moq.Times
using backend.Application.Common.Constants; // Added for ErrorSources
using backend.Domain.Common; // Added for BaseEvent

namespace backend.Application.UnitTests.FamilyLocations.Commands.CreateFamilyLocation;

public class CreateFamilyLocationCommandHandlerTests : TestBase
{
    private readonly CreateFamilyLocationCommandHandler _handler;

    public CreateFamilyLocationCommandHandlerTests()
    {
        _handler = new CreateFamilyLocationCommandHandler(_context, _mapper);
    }

    private Family CreateTestFamily(Guid familyId)
    {
        var family = Family.Create("Test Family", "TF", null, null, "Public", Guid.NewGuid());
        family.Id = familyId; // Assign the provided familyId
        return family;
    }

    [Fact]
    public async Task Handle_ShouldCreateFamilyLocation_WhenValidCommand()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        var command = new CreateFamilyLocationCommand
        {
            FamilyId = familyId,
            Name = "Home",
            Description = "Our first home",
            Latitude = 10.0,
            Longitude = 20.0,
            Address = "123 Main St",
            LocationType = LocationType.Homeland, // Changed from Residence
            Accuracy = LocationAccuracy.Exact,     // Changed from House
            Source = LocationSource.UserSelected  // Changed from UserGenerated
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var createdLocation = await _context.FamilyLocations.FindAsync(result.Value);
        createdLocation.Should().NotBeNull();
        createdLocation!.FamilyId.Should().Be(command.FamilyId);
        createdLocation.Name.Should().Be(command.Name);
        createdLocation.LocationType.Should().Be(command.LocationType);

        // Verify that the domain event was added
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(
            It.Is<List<BaseEvent>>(events => events.Any(e => e is FamilyLocationCreatedEvent))
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenInvalidCommand()
    {
        // Arrange
        var command = new CreateFamilyLocationCommand
        {
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
        // We need to validate the command separately because the handler assumes a valid command.
        // The validator is usually run before the handler.
        var validator = new CreateFamilyLocationCommandValidator();
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFamilyLocationCommand.FamilyId));
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFamilyLocationCommand.Name));
    }

    [Fact]
    public async Task Handle_ShouldRaiseFamilyLocationCreatedEvent()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        var command = new CreateFamilyLocationCommand
        {
            FamilyId = familyId,
            Name = "Park",
            Description = "Local park",
            Latitude = 30.0,
            Longitude = 40.0,
            Address = "Park Ave",
            LocationType = LocationType.EventLocation, // Changed from PublicPlace
            Accuracy = LocationAccuracy.Approximate,  // Changed from Street
            Source = LocationSource.UserSelected   // Changed from UserGenerated
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(
            It.Is<List<BaseEvent>>(events => events.Any(e => e is FamilyLocationCreatedEvent))
        ), Times.Once);
    }
}