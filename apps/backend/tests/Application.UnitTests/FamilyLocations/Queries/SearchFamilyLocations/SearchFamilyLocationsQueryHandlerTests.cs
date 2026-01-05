using System.Reflection; // Added for reflection-based property setting
using backend.Application.Common.Interfaces; // Add this using statement
using backend.Application.FamilyLocations; // For FamilyLocationDto
using backend.Application.FamilyLocations.Queries.SearchFamilyLocations;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.FamilyLocations.Queries.SearchFamilyLocations;

public class SearchFamilyLocationsQueryHandlerTests : TestBase
{
    private readonly SearchFamilyLocationsQueryHandler _handler;
    private readonly Mock<IPrivacyService> _mockPrivacyService;

    public SearchFamilyLocationsQueryHandlerTests()
    {
        _mockPrivacyService = new Mock<IPrivacyService>();
        // Default setup for privacy service to return the DTO as is (no filtering for basic tests)
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<FamilyLocationDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FamilyLocationDto dto, Guid familyId, CancellationToken token) => dto);
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<List<FamilyLocationDto>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<FamilyLocationDto> dtos, Guid familyId, CancellationToken token) => dtos);

        _handler = new SearchFamilyLocationsQueryHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object, _mockPrivacyService.Object);
    }

    private Family CreateTestFamily(Guid familyId, string name, string code)
    {
        var family = Family.Create(name, code, null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        return family;
    }

    private (FamilyLocation FamilyLocation, Location Location) CreateTestFamilyLocation(Guid familyId, Guid locationId, string name, LocationType type, LocationSource source, string? description = null, string? address = null)
    {
        var location = new Location(name, description ?? "Default Description", 1.0, 1.0, address ?? "Default Address", type, LocationAccuracy.Exact, source);
        SetPrivateProperty(location, "Id", locationId);

        var familyLocation = new FamilyLocation(familyId, location.Id);
        SetPrivateProperty(familyLocation, "Location", location);
        SetPrivateProperty(familyLocation, "Id", Guid.NewGuid());
        return (familyLocation, location);
    }

    private async Task SeedData(params (FamilyLocation FamilyLocation, Location Location)[] familyLocationsWithDetails)
    {
        foreach (var (familyLocation, location) in familyLocationsWithDetails)
        {
            if (await _context.Families.FindAsync(familyLocation.FamilyId) == null)
            {
                await _context.Families.AddAsync(CreateTestFamily(familyLocation.FamilyId, $"Family{familyLocation.FamilyId.ToString().Substring(0, 4)}", $"F{familyLocation.FamilyId.ToString().Substring(0, 4)}"));
            }
            // Add Location first if it's not already tracked
            if (_context.Entry(location).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
            {
                await _context.Locations.AddAsync(location);
            }
            await _context.FamilyLocations.AddAsync(familyLocation);
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
        result.Value.Items.Should().Contain(dto => dto.Id == loc1.FamilyLocation.Id);
        result.Value.Items.Should().Contain(dto => dto.Id == loc2.FamilyLocation.Id);
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
        result.Value!.Items.Should().ContainSingle(dto => dto.Id == loc1.FamilyLocation.Id);
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
        result.Value!.Items.Should().ContainSingle(dto => dto.Id == loc1.FamilyLocation.Id);
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
        result.Value!.Items.Should().ContainSingle(dto => dto.Id == loc1.FamilyLocation.Id);
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
        result.Value!.Items.Should().ContainSingle(dto => dto.Id == loc1.FamilyLocation.Id);
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
        result.Value!.Items.Should().ContainSingle(dto => dto.Id == loc1.FamilyLocation.Id);
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
        result.Value.Items.First().Location.Name.Should().Be("Alpha");
        result.Value.Items.Last().Location.Name.Should().Be("Beta");
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
