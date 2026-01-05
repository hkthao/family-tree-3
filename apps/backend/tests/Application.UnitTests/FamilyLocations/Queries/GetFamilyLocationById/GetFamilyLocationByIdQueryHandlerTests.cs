using backend.Application.Common.Constants; // Added for ErrorSources
using backend.Application.Common.Interfaces; // Add this using statement
using backend.Application.FamilyLocations; // For FamilyLocationDto
using backend.Application.FamilyLocations.Queries.GetFamilyLocationById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.FamilyLocations.Queries.GetFamilyLocationById;

public class GetFamilyLocationByIdQueryHandlerTests : TestBase
{
    private readonly GetFamilyLocationByIdQueryHandler _handler;
    private readonly Mock<IPrivacyService> _mockPrivacyService;

    public GetFamilyLocationByIdQueryHandlerTests()
    {
        _mockPrivacyService = new Mock<IPrivacyService>();
        // Default setup for privacy service to return the DTO as is (no filtering for basic tests)
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<FamilyLocationDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FamilyLocationDto dto, Guid familyId, CancellationToken token) => dto);

        _handler = new GetFamilyLocationByIdQueryHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object, _mockPrivacyService.Object);
    }

    private Family CreateTestFamily(Guid familyId)
    {
        var family = Family.Create("Test Family", "TF", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        return family;
    }

    private FamilyLocation CreateTestFamilyLocation(Guid familyId, Guid locationId, string name)
    {
        var location = new Location(name, "Test Description", 1.0, 1.0, "Test Address", LocationType.Homeland, LocationAccuracy.Exact, LocationSource.UserSelected);
        SetPrivateProperty(location, "Id", locationId);
        _context.Locations.Add(location); // Explicitly add Location to context
        _context.SaveChanges(); // Save changes to ensure location has an Id from context if not set manually

        var familyLocation = new FamilyLocation(familyId, location.Id);
        SetPrivateProperty(familyLocation, "Location", location);
        SetPrivateProperty(familyLocation, "Id", locationId); // Set FamilyLocation Id to match locationId for the query
        return familyLocation;
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
        result.Value.Location.Name.Should().Be(existingLocation.Location.Name);
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
