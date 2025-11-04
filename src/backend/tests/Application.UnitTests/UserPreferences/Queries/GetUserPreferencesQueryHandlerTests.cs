using backend.Application.Common.Constants;
using backend.Application.UnitTests.Common;
using backend.Application.UserPreferences.Queries.GetUserPreferences;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.UserPreferences;

/// <summary>
/// Kiểm thử đơn vị cho GetUserPreferencesQueryHandler.
/// </summary>
public class GetUserPreferencesQueryHandlerTests : TestBase
{
    private readonly GetUserPreferencesQueryHandler _handler;

    public GetUserPreferencesQueryHandlerTests()
    {
        _handler = new GetUserPreferencesQueryHandler(_context, _mockUser.Object, _mapper);
    }

    /// <summary>
    /// Kiểm tra khi UserProfile và UserPreference tồn tại, trả về tùy chọn của người dùng.
    /// </summary>
    [Fact]
    public async Task Handle_GivenUserProfileAndPreferenceExist_ShouldReturnUserPreferences()
    {
        // Arrange
        var authProviderId = Guid.NewGuid().ToString();
        var profileId = Guid.NewGuid();
        var user = new User(authProviderId, "test@example.com", "Test User", "Test", "User", "123456789", "avatar.png");
        user.UpdateProfile(authProviderId, "test@example.com", "Test User", "Test", "User", "123456789", "avatar.png");
        //user.Profile!.Id = profileId;
        user!.UpdatePreference(Theme.Light, Language.English);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _mockUser.Setup(x => x.UserId).Returns(user.Id);
        _mockUser.Setup(x => x.ProfileId).Returns(profileId);

        var query = new GetUserPreferencesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Theme.Should().Be(Theme.Light);
        result.Value!.Language.Should().Be(Language.English);
    }

    /// <summary>
    /// Kiểm tra khi UserProfile tồn tại nhưng UserPreference không tồn tại, trả về tùy chọn mặc định.
    /// </summary>
    [Fact]
    public async Task Handle_GivenUserProfileExistsButNoPreference_ShouldReturnDefaultPreferences()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var user = new User(userId.ToString(), "test@example.com", "Test User", "Test", "User", "123456789", "avatar.png");
        user.Id = userId;
        user.UpdateProfile(userId.ToString(), "test@example.com", "Test User", "Test", "User", "123456789", "avatar.png");
        user.Profile!.Id = profileId;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockUser.Setup(x => x.ProfileId).Returns(profileId);

        // Mapper sẽ không được gọi trong trường hợp này vì UserPreference là null,
        // nên không cần setup mockMapper cho trường hợp này.

        var query = new GetUserPreferencesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Theme.Should().Be(Theme.Dark);
        result.Value!.Language.Should().Be(Language.Vietnamese);
    }

    /// <summary>
    /// Kiểm tra khi UserProfile không tồn tại, trả về lỗi.
    /// </summary>
    [Fact]
    public async Task Handle_GivenUserProfileDoesNotExist_ShouldReturnFailureResult()
    {
        // Arrange
        _mockUser.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _mockUser.Setup(x => x.ProfileId).Returns(Guid.NewGuid()); // ProfileId không tồn tại trong DB

        var query = new GetUserPreferencesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(ErrorMessages.UserProfileNotFound);
    }
}
