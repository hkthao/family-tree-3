using AutoMapper;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Application.Identity.UserProfiles.Queries.GetCurrentUserProfile;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Queries.GetCurrentUserProfile;

public class GetCurrentUserProfileQueryHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetCurrentUserProfileQueryHandler _handler;

    public GetCurrentUserProfileQueryHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetCurrentUserProfileQueryHandler(_context, _currentUserMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserProfileNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        var query = new GetCurrentUserProfileQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.UserProfileNotFound);
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserProfile_WhenUserProfileExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userProfile = new UserProfile(userId, "ext1", "test@example.com", "Test User", "Test", "User", null, null);
        userProfile.Update("ext1", "test@example.com", "Test User", "Test", "User", null!, null!);
        await _context.UserProfiles.AddAsync(userProfile);
        await _context.SaveChangesAsync(CancellationToken.None);

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _mapperMock.Setup(m => m.Map<UserProfileDto>(It.IsAny<UserProfile>()))
            .Returns(new UserProfileDto { Id = userProfile.Id, ExternalId = "ext1", Email = "test@example.com", Name = "Test User" });

        var query = new GetCurrentUserProfileQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(userProfile.Id);
        result.Value.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_ShouldReturnUserProfileWithRoles_WhenRolesAreAvailable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userProfile = new UserProfile(userId, "ext1", "test@example.com", "Test User", "Test", "User", null, null);
        userProfile.Update("ext1", "test@example.com", "Test User", "Test", "User", null!, null!);
        await _context.UserProfiles.AddAsync(userProfile);
        await _context.SaveChangesAsync(CancellationToken.None);

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _currentUserMock.Setup(x => x.Roles).Returns(["Admin", "User"]);
        _mapperMock.Setup(m => m.Map<UserProfileDto>(It.IsAny<UserProfile>()))
            .Returns(new UserProfileDto { Id = userProfile.Id, ExternalId = "ext1", Email = "test@example.com", Name = "Test User" });

        var query = new GetCurrentUserProfileQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Roles.Should().Contain(new List<string> { "Admin", "User" });
    }
}
