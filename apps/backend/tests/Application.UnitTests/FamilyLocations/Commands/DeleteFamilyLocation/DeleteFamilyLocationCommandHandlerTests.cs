using backend.Application.Common.Models;
using backend.Application.FamilyLocations.Commands.DeleteFamilyLocation;
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
using backend.Infrastructure.Data; // Added for ApplicationDbContext
using backend.Application.Common.Constants; // Added for ErrorSources
using backend.Domain.Common; // Added for BaseEvent

namespace backend.Application.UnitTests.FamilyLocations.Commands.DeleteFamilyLocation;

public class DeleteFamilyLocationCommandHandlerTests : TestBase
{
    private readonly DeleteFamilyLocationCommandHandler _handler;

    public DeleteFamilyLocationCommandHandlerTests()
    {
        _handler = new DeleteFamilyLocationCommandHandler(_context);
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
            Description = "Test Description",
            Latitude = 1.0,
            Longitude = 1.0,
            Address = "Test Address",
            LocationType = LocationType.Homeland,
            Accuracy = LocationAccuracy.Exact,
            Source = LocationSource.UserSelected
        };
    }

    [Fact]
    public async Task Handle_ShouldDeleteFamilyLocation_WhenValidCommand()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);

        var locationId = Guid.NewGuid();
        var existingLocation = CreateTestFamilyLocation(familyId, locationId, "Location to Delete");
        await _context.FamilyLocations.AddAsync(existingLocation);
        await _context.SaveChangesAsync();

        var command = new DeleteFamilyLocationCommand(locationId);

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
        var handler = new DeleteFamilyLocationCommandHandler(_context);

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
        var existingLocation = CreateTestFamilyLocation(familyId, locationId, "Location to Delete");
        await _context.FamilyLocations.AddAsync(existingLocation);
        await _context.SaveChangesAsync();

        var command = new DeleteFamilyLocationCommand(locationId);

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
