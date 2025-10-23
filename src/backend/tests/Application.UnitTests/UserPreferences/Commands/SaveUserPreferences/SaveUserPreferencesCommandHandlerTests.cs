using AutoFixture;
using backend.Application.UnitTests.Common;
using backend.Application.UserPreferences.Commands.SaveUserPreferences;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.UserPreferences.Commands.SaveUserPreferences;

public class SaveUserPreferencesCommandHandlerTests : TestBase
{
    private readonly SaveUserPreferencesCommandHandler _handler;

    public SaveUserPreferencesCommandHandlerTests()
    {
        _handler = new SaveUserPreferencesCommandHandler(_context, _mockUser.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldCreateUserPreference_WhenUserPreferenceDoesNotExist()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng một UserPreference mới được tạo và lưu vào cơ sở dữ liệu
        // khi người dùng hiện tại chưa có UserPreference nào.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile giả lập với UserPreference là null.
        // 2. Thiết lập _mockUser để trả về UserProfileId của người dùng giả lập.
        // 3. Tạo một SaveUserPreferencesCommand với các giá trị mong muốn.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem UserPreference mới đã được tạo và lưu vào cơ sở dữ liệu với các giá trị chính xác.

        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfile = _fixture.Build<UserProfile>()
                                  .With(up => up.ExternalId, userId)
                                  .Without(up => up.UserPreference) // Đảm bảo UserPreference là null
                                  .Create();
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockUser.Setup(u => u.Id).Returns(userId);

        var command = new SaveUserPreferencesCommand
        {
            Theme = Theme.Dark,
            Language = Language.English,
            EmailNotificationsEnabled = true,
            SmsNotificationsEnabled = false,
            InAppNotificationsEnabled = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var savedUserPreference = _context.UserPreferences.FirstOrDefault(up => up.UserProfileId == userProfile.Id);
        savedUserPreference.Should().NotBeNull();
        savedUserPreference!.Theme.Should().Be(command.Theme);
        savedUserPreference.Language.Should().Be(command.Language);
        savedUserPreference.EmailNotificationsEnabled.Should().Be(command.EmailNotificationsEnabled);
        savedUserPreference.SmsNotificationsEnabled.Should().Be(command.SmsNotificationsEnabled);
        savedUserPreference.InAppNotificationsEnabled.Should().Be(command.InAppNotificationsEnabled);

        // 💡 Giải thích:
        // Test này đảm bảo rằng khi một người dùng chưa có bất kỳ tùy chọn nào,
        // hệ thống sẽ tạo một bản ghi UserPreference mới với các giá trị được cung cấp trong command
        // và liên kết nó với UserProfile của người dùng đó.
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingUserPreference_WhenUserPreferenceExists()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng một UserPreference hiện có được cập nhật chính xác
        // khi người dùng gửi một SaveUserPreferencesCommand.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile và một UserPreference hiện có cho người dùng.
        // 2. Thiết lập _mockUser để trả về UserProfileId của người dùng.
        // 3. Tạo một SaveUserPreferencesCommand với các giá trị mới.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem UserPreference hiện có đã được cập nhật với các giá trị mới.

        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userProfile = _fixture.Build<UserProfile>()
                                  .With(up => up.ExternalId, userId)
                                  .Create();
        var existingUserPreference = _fixture.Build<UserPreference>()
                                             .With(up => up.UserProfileId, userProfile.Id)
                                             .With(up => up.Theme, Theme.Light)
                                             .With(up => up.Language, Language.Vietnamese)
                                             .Create();

        _context.UserProfiles.Add(userProfile);
        _context.UserPreferences.Add(existingUserPreference);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockUser.Setup(u => u.Id).Returns(userId);

        var command = new SaveUserPreferencesCommand
        {
            Theme = Theme.Dark,
            Language = Language.English,
            EmailNotificationsEnabled = true,
            SmsNotificationsEnabled = true,
            InAppNotificationsEnabled = false
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedUserProfile = _context.UserProfiles.Include(up => up.UserPreference).FirstOrDefault(up => up.Id == userProfile.Id);
        updatedUserProfile.Should().NotBeNull();
        updatedUserProfile!.UserPreference.Should().NotBeNull();
        updatedUserProfile.UserPreference!.Theme.Should().Be(command.Theme);
        updatedUserProfile.UserPreference.Language.Should().Be(command.Language);
        updatedUserProfile.UserPreference.EmailNotificationsEnabled.Should().Be(command.EmailNotificationsEnabled);
        updatedUserProfile.UserPreference.SmsNotificationsEnabled.Should().Be(command.SmsNotificationsEnabled);
        updatedUserProfile.UserPreference.InAppNotificationsEnabled.Should().Be(command.InAppNotificationsEnabled);

        // 💡 Giải thích:
        // Test này đảm bảo rằng khi một người dùng đã có UserPreference,
        // hệ thống sẽ tìm và cập nhật bản ghi hiện có với các giá trị mới từ command.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotAuthenticated()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về một kết quả thất bại
        // khi người dùng không được xác thực (User.Id là null hoặc rỗng).

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập _mockUser để trả về null cho User.Id.
        // 2. Tạo một SaveUserPreferencesCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thất bại.
        // 2. Kiểm tra thông báo lỗi phù hợp.

        // Arrange
        _mockUser.Setup(u => u.Id).Returns((string)null!); // User is not authenticated

        var command = new SaveUserPreferencesCommand
        {
            Theme = Theme.Light,
            Language = Language.Vietnamese,
            EmailNotificationsEnabled = false,
            SmsNotificationsEnabled = false,
            InAppNotificationsEnabled = false
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User is not authenticated.");
        result.ErrorSource.Should().Be("Authentication");

        // 💡 Giải thích:
        // Test này kiểm tra trường hợp bảo mật cơ bản: nếu không có người dùng được xác thực,
        // yêu cầu lưu tùy chọn sẽ bị từ chối với thông báo lỗi rõ ràng.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserProfileNotFound()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về một kết quả thất bại
        // khi UserProfile của người dùng được xác thực không tìm thấy trong cơ sở dữ liệu.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập _mockUser để trả về một UserProfileId hợp lệ nhưng không tồn tại trong DB.
        // 2. Đảm bảo không có UserProfile nào trong DB khớp với ID này.
        // 3. Tạo một SaveUserPreferencesCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thất bại.
        // 2. Kiểm tra thông báo lỗi phù hợp.

        // Arrange
        var userId = Guid.NewGuid().ToString();
        _mockUser.Setup(u => u.Id).Returns(userId);

        // Ensure no UserProfile exists for this userId
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new SaveUserPreferencesCommand
        {
            Theme = Theme.Light,
            Language = Language.Vietnamese,
            EmailNotificationsEnabled = false,
            SmsNotificationsEnabled = false,
            InAppNotificationsEnabled = false
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User profile not found.");
        result.ErrorSource.Should().Be("NotFound");

        // 💡 Giải thích:
        // Test này đảm bảo rằng ngay cả khi người dùng được xác thực,
        // nếu hồ sơ người dùng của họ không tồn tại trong hệ thống,
        // yêu cầu sẽ thất bại để ngăn chặn việc tạo dữ liệu không hợp lệ.
    }
}
