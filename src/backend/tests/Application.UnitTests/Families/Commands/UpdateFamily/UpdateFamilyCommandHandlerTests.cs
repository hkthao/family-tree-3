using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.UpdateFamily;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Events.Families;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly UpdateFamilyCommandHandler _handler;

    public UpdateFamilyCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _handler = new UpdateFamilyCommandHandler(_context, _authorizationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateFamilyAndReturnSuccess_WhenAuthorizedAsAdmin()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var existingFamily = new Family { Id = familyId, Name = "Old Name", Code = "OLD", Visibility = "Public" };
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);

        var command = new UpdateFamilyCommand
        {
            Id = familyId,
            Name = "New Name",
            Description = "New Description",
            Address = "New Address",
            AvatarUrl = "http://new.avatar.com",
            Visibility = "Private",
            Code = "NEW"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedFamily = await _context.Families.FindAsync(familyId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedFamily.Should().NotBeNull();
        updatedFamily!.Name.Should().Be(command.Name);
        updatedFamily.Description.Should().Be(command.Description);
        updatedFamily.Address.Should().Be(command.Address);
        updatedFamily.AvatarUrl.Should().Be(command.AvatarUrl);
        updatedFamily.Visibility.Should().Be(command.Visibility);
        updatedFamily.Code.Should().Be(command.Code);
        updatedFamily.DomainEvents.Should().ContainSingle(e => e is FamilyUpdatedEvent);
        updatedFamily.DomainEvents.Should().ContainSingle(e => e is FamilyStatsUpdatedEvent);
    }

    [Fact]
    public async Task Handle_ShouldUpdateFamilyAndReturnSuccess_WhenAuthorizedAsManager()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var existingFamily = new Family { Id = familyId, Name = "Old Name", Code = "OLD", Visibility = "Public" };
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(false);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateFamilyCommand
        {
            Id = familyId,
            Name = "New Name",
            Visibility = "Private"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedFamily = await _context.Families.FindAsync(familyId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedFamily.Should().NotBeNull();
        updatedFamily!.Name.Should().Be(command.Name);
        updatedFamily.Visibility.Should().Be(command.Visibility);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyNotFound()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "Any Name", Visibility = "Public" };
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.FamilyNotFound, command.Id));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var existingFamily = new Family { Id = familyId, Name = "Old Name", Code = "OLD", Visibility = "Public" };
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        var command = new UpdateFamilyCommand { Id = familyId, Name = "New Name", Visibility = "Private" };

        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(false);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }
}