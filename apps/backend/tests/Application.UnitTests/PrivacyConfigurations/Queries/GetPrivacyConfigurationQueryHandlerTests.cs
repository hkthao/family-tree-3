using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.PrivacyConfigurations.Queries;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.PrivacyConfigurations.Queries;

public class GetPrivacyConfigurationQueryHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly GetPrivacyConfigurationQueryHandler _handler;

    public GetPrivacyConfigurationQueryHandlerTests() : base()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _handler = new GetPrivacyConfigurationQueryHandler(_context, _mapper, _authorizationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var query = new GetPrivacyConfigurationQuery(familyId);

        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        _authorizationServiceMock.Verify(x => x.CanAccessFamily(familyId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnExistingPrivacyConfigurationDto()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var existingConfig = new PrivacyConfiguration(familyId);
        existingConfig.UpdatePublicMemberProperties(new List<string> { "Property1", "Property2" });
        _context.PrivacyConfigurations.Add(existingConfig);
        await _context.SaveChangesAsync();

        var query = new GetPrivacyConfigurationQuery(familyId);
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FamilyId.Should().Be(familyId);
        result.Value.PublicMemberProperties.Should().BeEquivalentTo("Property1", "Property2");
        _authorizationServiceMock.Verify(x => x.CanAccessFamily(familyId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnDefaultPrivacyConfigurationDto_WhenNoneExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var query = new GetPrivacyConfigurationQuery(familyId);

        // Ensure no config exists for this familyId
        _context.PrivacyConfigurations.RemoveRange(_context.PrivacyConfigurations.Where(pc => pc.FamilyId == familyId));
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FamilyId.Should().Be(familyId);
        result.Value.PublicMemberProperties.Should().BeEquivalentTo(new List<string>()); // Default is empty list
        _authorizationServiceMock.Verify(x => x.CanAccessFamily(familyId), Times.Once);
    }
}
