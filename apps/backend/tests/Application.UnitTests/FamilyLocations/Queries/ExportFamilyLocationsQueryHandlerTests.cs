using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces; // Add this using statement
using backend.Application.FamilyLocations; // For FamilyLocationDto
using backend.Application.FamilyLocations.Queries.ExportFamilyLocations;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.FamilyLocations.Queries;

public class ExportFamilyLocationsQueryHandlerTests : TestBase
{
    private readonly ExportFamilyLocationsQueryHandler _handler;
    private readonly Mock<ILogger<ExportFamilyLocationsQueryHandler>> _mockLogger;
    private readonly Mock<IPrivacyService> _mockPrivacyService;

    private readonly Guid _testFamilyId = Guid.NewGuid();

    public ExportFamilyLocationsQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<ExportFamilyLocationsQueryHandler>>();
        _mockPrivacyService = new Mock<IPrivacyService>();
        // Default setup for privacy service to return the DTO as is (no filtering for basic tests)
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<FamilyLocationDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FamilyLocationDto dto, Guid familyId, CancellationToken token) => dto);
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<List<FamilyLocationDto>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<FamilyLocationDto> dtos, Guid familyId, CancellationToken token) => dtos);

        _handler = new ExportFamilyLocationsQueryHandler(_context, _mockAuthorizationService.Object, _mockLogger.Object, _mapper, _mockPrivacyService.Object);
    }

    [Fact]
    public async Task Handle_ShouldExportAllFamilyLocations_WhenUserIsAdmin()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        var location1 = new Location("Location 1", "Desc 1", 1.0, 1.0, "Addr 1", Domain.Enums.LocationType.Homeland, Domain.Enums.LocationAccuracy.Exact, Domain.Enums.LocationSource.UserSelected);
        var familyLocation1 = new FamilyLocation(_testFamilyId, location1.Id);
        SetPrivateProperty(familyLocation1, "Id", Guid.NewGuid());
        SetPrivateProperty(familyLocation1, "Location", location1);

        var location2 = new Location("Location 2", "Desc 2", 2.0, 2.0, "Addr 2", Domain.Enums.LocationType.Homeland, Domain.Enums.LocationAccuracy.Exact, Domain.Enums.LocationSource.UserSelected);
        var familyLocation2 = new FamilyLocation(_testFamilyId, location2.Id);
        SetPrivateProperty(familyLocation2, "Id", Guid.NewGuid());
        SetPrivateProperty(familyLocation2, "Location", location2);

        var locationOther = new Location("Other Family Location", "Desc Other", 3.0, 3.0, "Addr Other", Domain.Enums.LocationType.Homeland, Domain.Enums.LocationAccuracy.Exact, Domain.Enums.LocationSource.UserSelected);
        var familyLocationOther = new FamilyLocation(Guid.NewGuid(), locationOther.Id); // Other family
        SetPrivateProperty(familyLocationOther, "Id", Guid.NewGuid());
        SetPrivateProperty(familyLocationOther, "Location", locationOther);

        await _context.FamilyLocations.AddAsync(familyLocation1);
        await _context.FamilyLocations.AddAsync(familyLocation2);
        await _context.FamilyLocations.AddAsync(familyLocationOther);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new ExportFamilyLocationsQuery(_testFamilyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(2);
        result.Value.Should().Contain(fl => fl.Location.Name == "Location 1");
        result.Value.Should().Contain(fl => fl.Location.Name == "Location 2");
    }

    [Fact]
    public async Task Handle_ShouldExportAllFamilyLocations_WhenUserIsFamilyManager()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(_testFamilyId)).Returns(true);

        var location1 = new Location("Location 1", "Desc 1", 1.0, 1.0, "Addr 1", Domain.Enums.LocationType.Homeland, Domain.Enums.LocationAccuracy.Exact, Domain.Enums.LocationSource.UserSelected);
        var familyLocation1 = new FamilyLocation(_testFamilyId, location1.Id);
        SetPrivateProperty(familyLocation1, "Id", Guid.NewGuid());
        SetPrivateProperty(familyLocation1, "Location", location1);

        var location2 = new Location("Location 2", "Desc 2", 2.0, 2.0, "Addr 2", Domain.Enums.LocationType.Homeland, Domain.Enums.LocationAccuracy.Exact, Domain.Enums.LocationSource.UserSelected);
        var familyLocation2 = new FamilyLocation(_testFamilyId, location2.Id);
        SetPrivateProperty(familyLocation2, "Id", Guid.NewGuid());
        SetPrivateProperty(familyLocation2, "Location", location2);

        await _context.FamilyLocations.AddAsync(familyLocation1);
        await _context.FamilyLocations.AddAsync(familyLocation2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new ExportFamilyLocationsQuery(_testFamilyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(2);
        result.Value.Should().Contain(fl => fl.Location.Name == "Location 1");
        result.Value.Should().Contain(fl => fl.Location.Name == "Location 2");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoFamilyLocationsExistForFamily()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);
        // No locations added for _testFamilyId

        var query = new ExportFamilyLocationsQuery(_testFamilyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnAccessDenied_WhenUserIsNotAdminAndNotFamilyManager()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(_testFamilyId)).Returns(false);

        var location1 = new Location("Location 1", "Desc 1", 1.0, 1.0, "Addr 1", Domain.Enums.LocationType.Homeland, Domain.Enums.LocationAccuracy.Exact, Domain.Enums.LocationSource.UserSelected);
        var familyLocation1 = new FamilyLocation(_testFamilyId, location1.Id);
        SetPrivateProperty(familyLocation1, "Id", Guid.NewGuid());
        SetPrivateProperty(familyLocation1, "Location", location1);

        await _context.FamilyLocations.AddAsync(familyLocation1);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new ExportFamilyLocationsQuery(_testFamilyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }
}
