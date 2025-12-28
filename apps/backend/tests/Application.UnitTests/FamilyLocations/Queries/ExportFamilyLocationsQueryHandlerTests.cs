using AutoMapper;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.FamilyLocations;
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
    private readonly Mock<IMapper> _mockMapper;

    private readonly Guid _testFamilyId = Guid.NewGuid();

    public ExportFamilyLocationsQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<ExportFamilyLocationsQueryHandler>>();
        _mockMapper = new Mock<IMapper>();

        // Setup the mapper for ProjectTo
        _mockMapper.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<FamilyLocation, FamilyLocationDto>();
        }));

        _handler = new ExportFamilyLocationsQueryHandler(_context, _mockAuthorizationService.Object, _mockLogger.Object, _mockMapper.Object);
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
