using AutoFixture;
using backend.Application.Common.Constants;
using backend.Application.UserPreferences.Queries.GetUserPreferences;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.UserPreferences.Queries.GetUserPreferences;

/// <summary>
/// Bộ test cho GetUserPreferencesQueryHandler.
/// </summary>
public class GetUserPreferencesQueryHandlerTests : TestBase
{
    private readonly GetUserPreferencesQueryHandler _handler;

    public GetUserPreferencesQueryHandlerTests()
    {
        _handler = new GetUserPreferencesQueryHandler(_context, _mockUser.Object, _mapper);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Kiểm tra xem handler có trả về các tùy chọn mặc định khi người dùng không có UserPreference nào được lưu trữ hay không.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile hợp lệ nhưng không có UserPreference liên kết. Thiết lập _mockUser.Id để trả về ID của UserProfile này. Thêm UserProfile vào _context.
    ///    - Act: Gọi Handle của GetUserPreferencesQueryHandler.
    ///    - Assert: Kết quả phải là Success. Data của kết quả phải chứa các giá trị mặc định (Theme.Light, Language.English, EmailNotificationsEnabled = true, v.v.).
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler được thiết kế để cung cấp một tập hợp các tùy chọn mặc định nếu không tìm thấy tùy chọn người dùng cụ thể, đảm bảo ứng dụng luôn có cấu hình cơ bản để hoạt động.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnDefaultPreferences_WhenUserHasNoExistingPreference()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);

        var userProfile = new UserProfile
        {
            Id = userId,
            ExternalId = userId.ToString(),
            Email = "test@example.com",
            Name = "Test User",
            Phone = "1234567890",
            UserPreference = null
        };

        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync();

        var retrievedUserProfile = await _context.UserProfiles.FindAsync(userId);
        retrievedUserProfile.Should().NotBeNull();

        var query = new GetUserPreferencesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Theme.Should().Be(Theme.Light);
        result.Value.Language.Should().Be(Language.English);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Kiểm tra xem handler có trả về các tùy chọn người dùng hiện có khi chúng được lưu trữ hay không.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile hợp lệ. Tạo một UserPreference với các giá trị tùy chỉnh và liên kết nó với UserProfile. Thiết lập _mockUser.Id để trả về ID của UserProfile này. Thêm UserProfile và UserPreference vào _context.
    ///    - Act: Gọi Handle của GetUserPreferencesQueryHandler.
    ///    - Assert: Kết quả phải là Success. Data của kết quả phải khớp với các giá trị tùy chỉnh đã được lưu trữ.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải truy xuất và trả về chính xác các tùy chọn đã được người dùng cấu hình và lưu trữ trong cơ sở dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnExistingPreferences_WhenUserHasExistingPreference()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);

        var userPreference = _fixture.Build<UserPreference>()
            .With(up => up.Theme, Theme.Dark)
            .With(up => up.Language, Language.Vietnamese)
            .Create();

        var userProfile = new UserProfile
        {
            Id = userId,
            ExternalId = userId.ToString(),
            Email = "test@example.com",
            Name = "Test User",
            Phone = "1234567890",
            UserPreference = userPreference
        };
        userPreference.UserProfile = userProfile; // Đảm bảo liên kết hai chiều

        _context.UserProfiles.Add(userProfile);
        _context.UserPreferences.Add(userPreference);
        await _context.SaveChangesAsync();

        var retrievedUserProfile = await _context.UserProfiles.FindAsync(userId);
        retrievedUserProfile.Should().NotBeNull();

        var query = new GetUserPreferencesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Theme.Should().Be(Theme.Dark);
        result.Value.Language.Should().Be(Language.Vietnamese);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Kiểm tra xem handler có trả về lỗi UserProfileNotFound khi không tìm thấy UserProfile cho ID người dùng hiện tại hay không.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockUser.Id để trả về một ID không tồn tại trong cơ sở dữ liệu. Đảm bảo _context không chứa UserProfile nào với ID đó.
    ///    - Act: Gọi Handle của GetUserPreferencesQueryHandler.
    ///    - Assert: Kết quả phải là Failure. Error của kết quả phải là ErrorMessages.UserProfileNotFound.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Đây là một trường hợp lỗi quan trọng để đảm bảo rằng hệ thống xử lý đúng đắn khi không thể xác định người dùng hiện tại, ngăn chặn các lỗi không mong muốn hoặc truy cập dữ liệu sai.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnUserProfileNotFound_WhenUserProfileDoesNotExist()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(nonExistentUserId);

        var query = new GetUserPreferencesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.UserProfileNotFound);
    }
}