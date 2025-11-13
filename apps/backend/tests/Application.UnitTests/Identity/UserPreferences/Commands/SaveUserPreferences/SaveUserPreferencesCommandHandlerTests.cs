using backend.Application.Common.Interfaces;
using backend.Application.UnitTests.Common;
using backend.Application.UserPreferences.Commands.SaveUserPreferences;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.UserPreferences;

/// <summary>
/// Kiểm thử đơn vị cho SaveUserPreferencesCommandHandler.
/// Đảm bảo rằng lệnh lưu tùy chọn người dùng hoạt động đúng cách.
/// </summary>
public class SaveUserPreferencesCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly SaveUserPreferencesCommandHandler _handler;

    public SaveUserPreferencesCommandHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _handler = new SaveUserPreferencesCommandHandler(_context, _currentUserMock.Object);
    }

    /// <summary>
    /// Kiểm tra khi người dùng và tùy chọn tồn tại, tùy chọn sẽ được cập nhật thành công.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldUpdateUserPreferences_WhenUserAndPreferenceExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User(userId.ToString(), "test@example.com", "Test User", "Test", "User", "123456789", "avatar.png");
        user.Id = userId;
        user.UpdateProfile(userId.ToString(), "test@example.com", "Test User", "Test", "User", "123456789", "avatar.png");
        user.UpdatePreference(Theme.Light, Language.English);
        _context.Users.Add(user);
        await _context.SaveChangesAsync(CancellationToken.None);

        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var command = new SaveUserPreferencesCommand
        {
            Theme = Theme.Dark,
            Language = Language.Vietnamese
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedUser = await _context.Users
            .Include(u => u.Preference)
            .FirstOrDefaultAsync(u => u.Id == userId);
        updatedUser.Should().NotBeNull();
        updatedUser!.Preference.Should().NotBeNull();
        updatedUser.Preference!.Theme.Should().Be(Theme.Dark);
        updatedUser.Preference.Language.Should().Be(Language.Vietnamese);
    }

    /// <summary>
    /// Kiểm tra khi người dùng không tồn tại, lệnh sẽ trả về lỗi.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(nonExistentUserId);

        var command = new SaveUserPreferencesCommand
        {
            Theme = Theme.Dark,
            Language = Language.Vietnamese
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"User with ID {nonExistentUserId} not found.");
    }
}
