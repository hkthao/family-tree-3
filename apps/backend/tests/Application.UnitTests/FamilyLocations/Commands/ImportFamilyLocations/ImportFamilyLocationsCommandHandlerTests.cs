using backend.Application.Common.Constants;
using backend.Application.FamilyLocations.Commands.ImportFamilyLocations;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.FamilyLocations.Commands;

public class ImportFamilyLocationsCommandHandlerTests : TestBase
{
    private readonly ImportFamilyLocationsCommandHandler _handler;
    private readonly Mock<ILogger<ImportFamilyLocationsCommandHandler>> _mockLogger;

    private readonly Guid _testFamilyId = Guid.NewGuid();

    public ImportFamilyLocationsCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<ImportFamilyLocationsCommandHandler>>();

        _handler = new ImportFamilyLocationsCommandHandler(_context, _mockAuthorizationService.Object, _mockLogger.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldImportNewFamilyLocations_WhenUserIsAdminAndLocationsAreNew()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        var importItems = new List<ImportFamilyLocationItemDto>
        {
            new() { LocationName = "Location A", LocationDescription = "Desc A", LocationType = Domain.Enums.LocationType.Homeland, LocationAccuracy = Domain.Enums.LocationAccuracy.Exact, LocationSource = Domain.Enums.LocationSource.UserSelected },
            new() { LocationName = "Location B", LocationDescription = "Desc B", LocationType = Domain.Enums.LocationType.EventLocation, LocationAccuracy = Domain.Enums.LocationAccuracy.Estimated, LocationSource = Domain.Enums.LocationSource.Geocoded }
        };
        var command = new ImportFamilyLocationsCommand(_testFamilyId, importItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(2);

        _context.FamilyLocations.Should().HaveCount(2);
        _context.FamilyLocations.First().Location.Name.Should().Be("Location A");
        _context.FamilyLocations.First().FamilyId.Should().Be(_testFamilyId);
        _context.FamilyLocations.Last().Location.Name.Should().Be("Location B");
        _context.FamilyLocations.Last().FamilyId.Should().Be(_testFamilyId);
    }

    [Fact]
    public async Task Handle_ShouldImportNewFamilyLocations_WhenUserIsFamilyManager()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(_testFamilyId)).Returns(true);

        var importItems = new List<ImportFamilyLocationItemDto>
        {
            new() { LocationName = "Location C", LocationDescription = "Desc C", LocationType = Domain.Enums.LocationType.Homeland, LocationAccuracy = Domain.Enums.LocationAccuracy.Exact, LocationSource = Domain.Enums.LocationSource.UserSelected },
        };
        var command = new ImportFamilyLocationsCommand(_testFamilyId, importItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(1);
        _context.FamilyLocations.Should().HaveCount(1);
        _context.FamilyLocations.First().Location.Name.Should().Be("Location C");
    }

    [Fact]
    public async Task Handle_ShouldSkipExistingFamilyLocations_WhenLocationsAlreadyExist()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        var existingLocation = new Location("Existing Location", "Existing Desc", 1.0, 1.0, "Addr 1", Domain.Enums.LocationType.Homeland, Domain.Enums.LocationAccuracy.Exact, Domain.Enums.LocationSource.UserSelected);
        var existingFamilyLocation = new FamilyLocation(_testFamilyId, existingLocation.Id);
        SetPrivateProperty(existingFamilyLocation, "Id", Guid.NewGuid());
        SetPrivateProperty(existingFamilyLocation, "Location", existingLocation);
        await _context.FamilyLocations.AddAsync(existingFamilyLocation);
        await _context.SaveChangesAsync(CancellationToken.None);

        var importItems = new List<ImportFamilyLocationItemDto>
        {
            new() { LocationName = "Existing Location", LocationDescription = "Updated Desc", LocationType = Domain.Enums.LocationType.Homeland, LocationAccuracy = Domain.Enums.LocationAccuracy.Exact, LocationSource = Domain.Enums.LocationSource.UserSelected }, // Should be skipped
            new() { LocationName = "New Location", LocationDescription = "New Desc", LocationType = Domain.Enums.LocationType.EventLocation, LocationAccuracy = Domain.Enums.LocationAccuracy.Estimated, LocationSource = Domain.Enums.LocationSource.Geocoded }
        };
        var command = new ImportFamilyLocationsCommand(_testFamilyId, importItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(1); // Only the new location should be imported

        _context.FamilyLocations.Should().HaveCount(2); // Existing + 1 new
        _context.FamilyLocations.Any(fl => fl.Location.Name == "Existing Location" && fl.FamilyId == _testFamilyId).Should().BeTrue();
        _context.FamilyLocations.Any(fl => fl.Location.Name == "New Location" && fl.FamilyId == _testFamilyId).Should().BeTrue();
        _context.FamilyLocations.First(fl => fl.Location.Name == "Existing Location").Location.Description.Should().Be("Existing Desc"); // Should not be updated
    }

    [Fact]
    public async Task Handle_ShouldReturnAccessDenied_WhenUserIsNotAdminAndNotFamilyManager()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(_testFamilyId)).Returns(false);

        var importItems = new List<ImportFamilyLocationItemDto>
        {
            new() { LocationName = "Location X", LocationDescription = "Desc X", LocationType = Domain.Enums.LocationType.Homeland, LocationAccuracy = Domain.Enums.LocationAccuracy.Exact, LocationSource = Domain.Enums.LocationSource.UserSelected },
        };
        var command = new ImportFamilyLocationsCommand(_testFamilyId, importItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        _context.FamilyLocations.Should().BeEmpty(); // No locations should be imported
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithEmptyList_WhenImportingEmptyList()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);
        var command = new ImportFamilyLocationsCommand(_testFamilyId, new List<ImportFamilyLocationItemDto>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        _context.FamilyLocations.Should().BeEmpty();
    }
}
