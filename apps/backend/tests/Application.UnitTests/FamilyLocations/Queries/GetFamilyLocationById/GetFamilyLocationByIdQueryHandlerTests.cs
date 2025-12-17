using backend.Application.Common.Models;
using backend.Application.FamilyLocations.Queries.GetFamilyLocationById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Application.Common.Constants; // Added for ErrorSources
using backend.Domain.Common; // Added for BaseEvent

namespace backend.Application.UnitTests.FamilyLocations.Queries.GetFamilyLocationById;

public class GetFamilyLocationByIdQueryHandlerTests : TestBase
{
    private readonly GetFamilyLocationByIdQueryHandler _handler;

    public GetFamilyLocationByIdQueryHandlerTests()
    {
        _handler = new GetFamilyLocationByIdQueryHandler(_context, _mapper);
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
    public async Task Handle_ShouldReturnFamilyLocationDto_WhenLocationExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);

        var locationId = Guid.NewGuid();
        var existingLocation = CreateTestFamilyLocation(familyId, locationId, "Existing Location");
        await _context.FamilyLocations.AddAsync(existingLocation);
        await _context.SaveChangesAsync();

        var query = new GetFamilyLocationByIdQuery(locationId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(locationId);
        result.Value.Name.Should().Be(existingLocation.Name);
        result.Value.FamilyId.Should().Be(familyId);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyLocationNotFound()
    {
        // Arrange
        var query = new GetFamilyLocationByIdQuery(Guid.NewGuid()); // Non-existent ID

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found.");
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }
}
