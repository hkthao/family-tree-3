using backend.Application.Common.Models;
using backend.Application.FamilyLocations.Queries.SearchFamilyLocations;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Application.Common.Constants; // Added for ErrorSources
using backend.Domain.Common; // Added for BaseEvent

namespace backend.Application.UnitTests.FamilyLocations.Queries.SearchFamilyLocations;

public class SearchFamilyLocationsQueryHandlerTests : TestBase
{
    private readonly SearchFamilyLocationsQueryHandler _handler;

    public SearchFamilyLocationsQueryHandlerTests()
    {
        _handler = new SearchFamilyLocationsQueryHandler(_context, _mapper);
    }

    private Family CreateTestFamily(Guid familyId, string name, string code)
    {
        var family = Family.Create(name, code, null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        return family;
    }

    private FamilyLocation CreateTestFamilyLocation(Guid familyId, Guid locationId, string name, LocationType type, LocationSource source, string? description = null, string? address = null)
    {
        return new FamilyLocation
        {
            Id = locationId,
            FamilyId = familyId,
            Name = name,
            Description = description,
            Latitude = 1.0,
            Longitude = 1.0,
            Address = address,
            LocationType = type,
            Accuracy = LocationAccuracy.Exact,
            Source = source
        };
    }

    private async Task SeedData(params FamilyLocation[] locations)
    {
        foreach (var loc in locations)
        {
            if (await _context.Families.FindAsync(loc.FamilyId) == null)
            {
                await _context.Families.AddAsync(CreateTestFamily(loc.FamilyId, $"Family{loc.FamilyId.ToString().Substring(0,4)}", $"F{loc.FamilyId.ToString().Substring(0,4)}"));
            }
            await _context.FamilyLocations.AddAsync(loc);
        }
        await _context.SaveChangesAsync();
    }


    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_WhenNoFilters()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var loc1 = CreateTestFamilyLocation(familyId, Guid.NewGuid(), "Location A", LocationType.Homeland, LocationSource.UserSelected);
        var loc2 = CreateTestFamilyLocation(familyId, Guid.NewGuid(), "Location B", LocationType.Homeland, LocationSource.Geocoded);
        await SeedData(loc1, loc2);

        var query = new SearchFamilyLocationsQuery { Page = 1, ItemsPerPage = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalItems.Should().Be(2);
        result.Value.Items.Should().Contain(dto => dto.Id == loc1.Id);
        result.Value.Items.Should().Contain(dto => dto.Id == loc2.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredByFamilyId()
    {
        // Arrange
        var familyId1 = Guid.NewGuid();
        var familyId2 = Guid.NewGuid();
        var loc1 = CreateTestFamilyLocation(familyId1, Guid.NewGuid(), "Location 1", LocationType.Homeland, LocationSource.UserSelected);
        var loc2 = CreateTestFamilyLocation(familyId2, Guid.NewGuid(), "Location 2", LocationType.Homeland, LocationSource.Geocoded);
        await SeedData(loc1, loc2);

        var query = new SearchFamilyLocationsQuery { FamilyId = familyId1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().ContainSingle(dto => dto.Id == loc1.Id);
        result.Value.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredBySearchQuery_OnName()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var loc1 = CreateTestFamilyLocation(familyId, Guid.NewGuid(), "Home Base", LocationType.Homeland, LocationSource.UserSelected);
        var loc2 = CreateTestFamilyLocation(familyId, Guid.NewGuid(), "Workplace", LocationType.Other, LocationSource.Geocoded);
        await SeedData(loc1, loc2);

        var query = new SearchFamilyLocationsQuery { SearchQuery = "home" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().ContainSingle(dto => dto.Id == loc1.Id);
        result.Value.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredBySearchQuery_OnDescription()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var loc1 = CreateTestFamilyLocation(familyId, Guid.NewGuid(), "Location 1", LocationType.Homeland, LocationSource.UserSelected, description: "A beautiful place.");
        var loc2 = CreateTestFamilyLocation(familyId, Guid.NewGuid(), "Location 2", LocationType.Homeland, LocationSource.UserSelected, description: "An ugly place.");
        await SeedData(loc1, loc2);

        var query = new SearchFamilyLocationsQuery { SearchQuery = "beautiful" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().ContainSingle(dto => dto.Id == loc1.Id);
        result.Value.TotalItems.Should().Be(1);
    }


    [Fact]
    public async Task Handle_ShouldReturnFilteredByLocationType()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var loc1 = CreateTestFamilyLocation(familyId, Guid.NewGuid(), "Residence", LocationType.Homeland, LocationSource.UserSelected);
        var loc2 = CreateTestFamilyLocation(familyId, Guid.NewGuid(), "Work", LocationType.Other, LocationSource.Geocoded);
        await SeedData(loc1, loc2);

        var query = new SearchFamilyLocationsQuery { LocationType = LocationType.Homeland };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().ContainSingle(dto => dto.Id == loc1.Id);
        result.Value.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredByLocationSource()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var loc1 = CreateTestFamilyLocation(familyId, Guid.NewGuid(), "User Location", LocationType.Homeland, LocationSource.UserSelected);
        var loc2 = CreateTestFamilyLocation(familyId, Guid.NewGuid(), "System Location", LocationType.Other, LocationSource.Geocoded);
        await SeedData(loc1, loc2);

        var query = new SearchFamilyLocationsQuery { Source = LocationSource.UserSelected };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().ContainSingle(dto => dto.Id == loc1.Id);
        result.Value.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldApplySortingAndPagination()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var locA = CreateTestFamilyLocation(familyId, Guid.NewGuid(), "Alpha", LocationType.Homeland, LocationSource.UserSelected);
        var locB = CreateTestFamilyLocation(familyId, Guid.NewGuid(), "Beta", LocationType.Other, LocationSource.Geocoded);
        var locC = CreateTestFamilyLocation(familyId, Guid.NewGuid(), "Gamma", LocationType.Homeland, LocationSource.UserSelected);
        await SeedData(locC, locB, locA); // Add in different order

        var query = new SearchFamilyLocationsQuery { Page = 1, ItemsPerPage = 2, SortBy = "name", SortOrder = "asc" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalItems.Should().Be(3);
        result.Value.Items.First().Name.Should().Be("Alpha");
        result.Value.Items.Last().Name.Should().Be("Beta");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoMatchingLocations()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var loc1 = CreateTestFamilyLocation(familyId, Guid.NewGuid(), "Location A", LocationType.Homeland, LocationSource.UserSelected);
        await SeedData(loc1);

        var query = new SearchFamilyLocationsQuery { SearchQuery = "NonExistent" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalItems.Should().Be(0);
    }
}