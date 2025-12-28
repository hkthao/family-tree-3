using backend.Application.Common.Constants;
using backend.Application.FamilyLocations.Queries.ExportFamilyLocations;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using backend.Application.Common.Interfaces; // Add this using statement
using backend.Application.FamilyLocations; // For FamilyLocationDto

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

        await _context.FamilyLocations.AddAsync(new FamilyLocation { Id = Guid.NewGuid(), FamilyId = _testFamilyId, Name = "Location 1" });
        await _context.FamilyLocations.AddAsync(new FamilyLocation { Id = Guid.NewGuid(), FamilyId = _testFamilyId, Name = "Location 2" });
        await _context.FamilyLocations.AddAsync(new FamilyLocation { Id = Guid.NewGuid(), FamilyId = Guid.NewGuid(), Name = "Other Family Location" }); // Other family
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new ExportFamilyLocationsQuery(_testFamilyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(2);
        result.Value.Should().Contain(fl => fl.Name == "Location 1");
        result.Value.Should().Contain(fl => fl.Name == "Location 2");
    }

    [Fact]
    public async Task Handle_ShouldExportAllFamilyLocations_WhenUserIsFamilyManager()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(_testFamilyId)).Returns(true);

        await _context.FamilyLocations.AddAsync(new FamilyLocation { Id = Guid.NewGuid(), FamilyId = _testFamilyId, Name = "Location 1" });
        await _context.FamilyLocations.AddAsync(new FamilyLocation { Id = Guid.NewGuid(), FamilyId = _testFamilyId, Name = "Location 2" });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new ExportFamilyLocationsQuery(_testFamilyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(2);
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

        await _context.FamilyLocations.AddAsync(new FamilyLocation { Id = Guid.NewGuid(), FamilyId = _testFamilyId, Name = "Location 1" });
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
