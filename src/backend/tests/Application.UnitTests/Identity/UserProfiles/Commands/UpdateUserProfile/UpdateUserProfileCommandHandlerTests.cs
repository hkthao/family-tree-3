using backend.Application.Common.Constants;
using AutoFixture.AutoMoq;
using backend.Application.Identity.Commands.UpdateUserProfile;
using backend.Application.Identity.UserProfiles.Commands.UpdateUserProfile;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Commands.UpdateUserProfile;

/// <summary>
/// Bộ test cho UpdateUserProfileCommandHandler.
/// </summary>
public class UpdateUserProfileCommandHandlerTests : TestBase
{
    private readonly UpdateUserProfileCommandHandler _handler;

    public UpdateUserProfileCommandHandlerTests()
    {
        _fixture.Customize(new AutoMoqCustomization());

        _handler = new UpdateUserProfileCommandHandler(
            _context
        );
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi định dạng Id không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateUserProfileCommand với Id có định dạng không phải GUID.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra kết quả trả về là thất bại, với thông báo lỗi là ErrorMessages.InvalidUserIdFormat
    ///              và ErrorSource là ErrorSources.Validation.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Id phải là một GUID hợp lệ để tìm kiếm hồ sơ người dùng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenInvalidIdFormat()
    {
        // Arrange
        var command = new UpdateUserProfileCommand { Id = "invalid-guid", Name = "Test", Email = "test@example.com" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(ErrorMessages.InvalidUserIdFormat);
        result.ErrorSource.Should().Be(ErrorSources.Validation);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenUserProfileNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy hồ sơ người dùng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateUserProfileCommand với Id hợp lệ nhưng không tồn tại trong DB.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = "Test", Email = "test@example.com" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
        result.ErrorSource.Should().Be("NotFound");
        // 💡 Giải thích: Không thể cập nhật hồ sơ người dùng nếu không tìm thấy nó.
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserProfileSuccessfully()
    {
        // 🎯 Mục tiêu của test: Xác minh handler cập nhật thành công hồ sơ người dùng hiện có.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UserProfile hiện có trong Context. Tạo một UpdateUserProfileCommand với Id của UserProfile đó và các giá trị mới.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công. Xác minh UserProfile trong Context đã được cập nhật với các giá trị mới.
        var userProfileId = Guid.NewGuid();
        var existingUserProfile = new UserProfile
        {
            Id = userProfileId,
            ExternalId = Guid.NewGuid().ToString(),
            Email = "old@example.com",
            Name = "Old Name",
            FirstName = "Old",
            LastName = "Name",
            Phone = "1234567890",
            Avatar = "http://old.com/avatar.jpg"
        };
        _context.UserProfiles.Add(existingUserProfile);
        await _context.SaveChangesAsync();

        var newName = "New Name";
        var newEmail = "new@example.com";
        var newAvatar = "http://new.com/avatar.jpg";

        var command = new UpdateUserProfileCommand
        {
            Id = userProfileId.ToString(),
            Name = newName,
            Email = newEmail,
            Avatar = newAvatar
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedUserProfile = await _context.UserProfiles.FindAsync(userProfileId);
        updatedUserProfile.Should().NotBeNull();
        updatedUserProfile!.Name.Should().Be(newName);
        updatedUserProfile.Email.Should().Be(newEmail);
        updatedUserProfile.Avatar.Should().Be(newAvatar);
        // 💡 Giải thích: Handler phải cập nhật thành công các thuộc tính của hồ sơ người dùng và lưu các thay đổi vào cơ sở dữ liệu.
    }
}
