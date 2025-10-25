using AutoFixture;
using backend.Application.UnitTests.Common;
using backend.Application.UserPreferences.Queries.GetUserPreferences;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.UserPreferences.Queries.GetUserPreferences;

public class GetUserPreferencesQueryHandlerTests : TestBase
{
    private readonly GetUserPreferencesQueryHandler _handler;

    public GetUserPreferencesQueryHandlerTests()
    {
        _handler = new GetUserPreferencesQueryHandler(_context, _mockUser.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserPreferences_WhenUserPreferencesExist()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về các tùy chọn người dùng hiện có
        // khi chúng tồn tại trong cơ sở dữ liệu.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile và một UserPreference với các giá trị cụ thể.
        // 2. Thiết lập _mockUser để trả về UserProfileId của người dùng.
        // 3. Tạo một GetUserPreferencesQuery.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem UserPreferenceDto trả về có chứa các giá trị chính xác.

        // Arrange
        var userId = Guid.NewGuid();
        var userProfile = _fixture.Build<UserProfile>()
                                  .Create();
        var existingUserPreference = _fixture.Build<UserPreference>()
                                             .With(up => up.UserProfileId, userProfile.Id)
                                             .With(up => up.Theme, Theme.Dark)
                                             .With(up => up.Language, Language.English)
                                             .With(up => up.EmailNotificationsEnabled, true)
                                             .With(up => up.SmsNotificationsEnabled, false)
                                             .With(up => up.InAppNotificationsEnabled, true)
                                             .Create();
        userProfile.UserPreference = existingUserPreference;
        _context.UserProfiles.Add(userProfile);
        _context.UserPreferences.Add(existingUserPreference);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockUser.Setup(u => u.Id).Returns(userId);

        var query = new GetUserPreferencesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Theme.Should().Be(existingUserPreference.Theme);
        result.Value.Language.Should().Be(existingUserPreference.Language);
        result.Value.EmailNotificationsEnabled.Should().Be(existingUserPreference.EmailNotificationsEnabled);
        result.Value.SmsNotificationsEnabled.Should().Be(existingUserPreference.SmsNotificationsEnabled);
        result.Value.InAppNotificationsEnabled.Should().Be(existingUserPreference.InAppNotificationsEnabled);

        // 💡 Giải thích:
        // Test này đảm bảo rằng khi người dùng có UserPreference đã lưu,
        // handler sẽ truy xuất và trả về các tùy chọn đó một cách chính xác.
    }

    [Fact]
    public async Task Handle_ShouldReturnDefaultPreferences_WhenUserPreferencesDoNotExist()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về các tùy chọn mặc định
        // khi người dùng chưa có UserPreference nào được lưu trong cơ sở dữ liệu.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile nhưng không tạo UserPreference cho nó.
        // 2. Thiết lập _mockUser để trả về UserProfileId của người dùng.
        // 3. Tạo một GetUserPreferencesQuery.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem UserPreferenceDto trả về có chứa các giá trị mặc định.

        // Arrange
        var userId = Guid.NewGuid();
        var userProfile = _fixture.Build<UserProfile>()
                                  .Without(up => up.UserPreference) // Đảm bảo UserPreference là null
                                  .Create();
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockUser.Setup(u => u.Id).Returns(userId);

        var query = new GetUserPreferencesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Theme.Should().Be(Theme.Light);
        result.Value.Language.Should().Be(Language.English);
        result.Value.EmailNotificationsEnabled.Should().BeTrue();
        result.Value.SmsNotificationsEnabled.Should().BeFalse();
        result.Value.InAppNotificationsEnabled.Should().BeTrue();

        // 💡 Giải thích:
        // Test này đảm bảo rằng nếu không tìm thấy tùy chọn người dùng đã lưu,
        // hệ thống sẽ cung cấp một bộ tùy chọn mặc định để đảm bảo ứng dụng luôn có trạng thái hợp lệ.
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
        // 2. Tạo một GetUserPreferencesQuery.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thất bại.
        // 2. Kiểm tra thông báo lỗi phù hợp.

        // Arrange
        _mockUser.Setup(u => u.Id).Returns((Guid?)null!); // User is not authenticated

        var query = new GetUserPreferencesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User is not authenticated.");
        result.ErrorSource.Should().Be("Authentication");

        // 💡 Giải thích:
        // Test này kiểm tra trường hợp bảo mật cơ bản: nếu không có người dùng được xác thực,
        // yêu cầu truy vấn tùy chọn sẽ bị từ chối với thông báo lỗi rõ ràng.
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
        // 3. Tạo một GetUserPreferencesQuery.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thất bại.
        // 2. Kiểm tra thông báo lỗi phù hợp.

        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);

        // Ensure no UserProfile exists for this userId
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetUserPreferencesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User profile not found.");
        result.ErrorSource.Should().Be("NotFound");

        // 💡 Giải thích:
        // Test này đảm bảo rằng ngay cả khi người dùng được xác thực,
        // nếu hồ sơ người dùng của họ không tồn tại trong hệ thống,
        // yêu cầu sẽ thất bại để ngăn chặn việc truy vấn dữ liệu không hợp lệ.
    }
}
