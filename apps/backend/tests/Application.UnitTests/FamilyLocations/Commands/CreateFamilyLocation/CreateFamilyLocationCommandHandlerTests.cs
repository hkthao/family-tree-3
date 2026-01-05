using backend.Application.Common.Constants; // ADDED for ErrorMessages and ErrorSources
using backend.Application.FamilyLocations.Commands.CreateFamilyLocation;
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // Added for BaseEvent
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Events;
using FluentAssertions;
using Moq; // Added for Moq.Times
using Xunit;

namespace backend.Application.UnitTests.FamilyLocations.Commands.CreateFamilyLocation;

public class CreateFamilyLocationCommandHandlerTests : TestBase
{
    private readonly CreateFamilyLocationCommandHandler _handler;

    public CreateFamilyLocationCommandHandlerTests()
    {
        _handler = new CreateFamilyLocationCommandHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object);
    }

    private Family CreateTestFamily(Guid familyId, Guid creatorUserId)
    {
        var family = Family.Create("Test Family", "TF", null, null, "Public", creatorUserId);
        family.Id = familyId; // Assign the provided familyId
        return family;
    }

    [Fact]
    public async Task Handle_ShouldCreateFamilyLocationAndRaiseEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true);

        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId, userId); // Use userId as creator
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        var command = new CreateFamilyLocationCommand
        {
            FamilyId = familyId,
            LocationName = "Home",
            LocationDescription = "Our first home",
            LocationLatitude = 10.0,
            LocationLongitude = 20.0,
            LocationAddress = "123 Main St",
            LocationType = LocationType.Homeland,
            LocationAccuracy = LocationAccuracy.Exact,
            LocationSource = LocationSource.UserSelected
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var createdLocation = await _context.FamilyLocations.FindAsync(result.Value);
        createdLocation.Should().NotBeNull();
        createdLocation!.FamilyId.Should().Be(command.FamilyId);
        createdLocation.Location.Name.Should().Be(command.LocationName);
        createdLocation.Location.LocationType.Should().Be(command.LocationType);

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
            LocationName = "", // Invalid Name
            LocationDescription = "A description",
            LocationLatitude = 10.0,
            LocationLongitude = 20.0,
            LocationAddress = "123 Main St",
            LocationType = LocationType.Homeland,
            LocationAccuracy = LocationAccuracy.Exact,
            LocationSource = LocationSource.UserSelected
        };

        // Act
        // We need to validate the command separately because the handler assumes a valid command.
        // The validator is usually run before the handler.
        var validator = new CreateFamilyLocationCommandValidator();
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFamilyLocationCommand.FamilyId));
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFamilyLocationCommand.LocationName));
    }

    [Fact]
    public async Task Handle_ShouldRaiseFamilyLocationCreatedEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true);

        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId, userId); // Use userId as creator
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        var command = new CreateFamilyLocationCommand
        {
            FamilyId = familyId,
            LocationName = "Park",
            LocationDescription = "Local park",
            LocationLatitude = 30.0,
            LocationLongitude = 40.0,
            LocationAddress = "Park Ave",
            LocationType = LocationType.EventLocation, // Changed from PublicPlace
            LocationAccuracy = LocationAccuracy.Approximate,  // Changed from Street
            LocationSource = LocationSource.UserSelected   // Changed from UserGenerated
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(
            It.Is<List<BaseEvent>>(events => events.Any(e => e is FamilyLocationCreatedEvent))
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        _mockUser.Setup(x => x.IsAuthenticated).Returns(false); // User is not authenticated
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true); // Authorization not relevant here

        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId, Guid.NewGuid()); // Family owned by another user
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        var command = new CreateFamilyLocationCommand
        {
            FamilyId = familyId,
            LocationName = "Unauthorized Location"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.Unauthorized);
        result.ErrorSource.Should().Be(ErrorSources.Authentication);
        _context.FamilyLocations.Should().BeEmpty(); // No location should be created
    }

    [Fact]
    public async Task Handle_ShouldReturnForbidden_WhenUserCannotAccessFamily()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(false); // User cannot access family

        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId, Guid.NewGuid()); // Family owned by another user
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        var command = new CreateFamilyLocationCommand
        {
            FamilyId = familyId,
            LocationName = "Forbidden Location"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        _context.FamilyLocations.Should().BeEmpty(); // No location should be created
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenFamilyDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true); // User can access (mocked)

        var nonExistentFamilyId = Guid.NewGuid(); // Family does not exist
        var command = new CreateFamilyLocationCommand
        {
            FamilyId = nonExistentFamilyId,
            LocationName = "Location for Non-existent Family"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.FamilyNotFound, nonExistentFamilyId));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
        _context.FamilyLocations.Should().BeEmpty(); // No location should be created
    }
}
