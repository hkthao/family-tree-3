using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.PrivacyConfigurations.Commands;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.PrivacyConfigurations.Commands;

public class UpdatePrivacyConfigurationCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly UpdatePrivacyConfigurationCommandHandler _handler;

    public UpdatePrivacyConfigurationCommandHandlerTests() : base()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _handler = new UpdatePrivacyConfigurationCommandHandler(_context, _authorizationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new UpdatePrivacyConfigurationCommand(familyId, new List<string> { "PropertyA" });

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        _authorizationServiceMock.Verify(x => x.CanManageFamily(familyId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingPrivacyConfiguration()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var existingConfig = new PrivacyConfiguration(familyId);
        existingConfig.UpdatePublicMemberProperties(new List<string> { "OldProperty" });
        _context.PrivacyConfigurations.Add(existingConfig);
        await _context.SaveChangesAsync();

        var command = new UpdatePrivacyConfigurationCommand(familyId, new List<string> { "NewProperty1", "NewProperty2" });

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedConfig = await _context.PrivacyConfigurations.FirstOrDefaultAsync(pc => pc.FamilyId == familyId);
        updatedConfig.Should().NotBeNull();
        updatedConfig!.PublicMemberProperties.Should().Be("NewProperty1,NewProperty2");
        _authorizationServiceMock.Verify(x => x.CanManageFamily(familyId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateNewPrivacyConfiguration_WhenNoneExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new UpdatePrivacyConfigurationCommand(familyId, new List<string> { "PropertyX", "PropertyY" });

        // Ensure no config exists for this familyId
        _context.PrivacyConfigurations.RemoveRange(_context.PrivacyConfigurations.Where(pc => pc.FamilyId == familyId));
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var newConfig = await _context.PrivacyConfigurations.FirstOrDefaultAsync(pc => pc.FamilyId == familyId);
        newConfig.Should().NotBeNull();
        newConfig!.PublicMemberProperties.Should().Be("PropertyX,PropertyY");
        _authorizationServiceMock.Verify(x => x.CanManageFamily(familyId), Times.Once);
    }
}
