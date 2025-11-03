using AutoMapper;
using backend.Application.Common.Constants;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Application.Identity.UserProfiles.Queries.GetUserProfileById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Queries.GetUserProfileById;

public class GetUserProfileByIdQueryHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetUserProfileByIdQueryHandler _handler;

    public GetUserProfileByIdQueryHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _handler = new GetUserProfileByIdQueryHandler(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserProfileNotFound()
    {
        // Arrange
        var query = new GetUserProfileByIdQuery { Id = Guid.NewGuid() };

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
        var userProfile = new UserProfile(userId);
        userProfile.Update("ext1", "test@example.com", "Test User", "Test", "User", null!, null!);
        await _context.UserProfiles.AddAsync(userProfile);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mapperMock.Setup(m => m.Map<UserProfileDto>(It.IsAny<UserProfile>()))
            .Returns(new UserProfileDto { Id = userProfile.Id, ExternalId = "ext1", Email = "test@example.com", Name = "Test User" });

        var query = new GetUserProfileByIdQuery { Id = userProfile.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(userProfile.Id);
        result.Value.Email.Should().Be("test@example.com");
    }
}
