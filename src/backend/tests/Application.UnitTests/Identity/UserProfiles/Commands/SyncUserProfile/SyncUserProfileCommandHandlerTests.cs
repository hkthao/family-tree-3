using System.Security.Claims;
using AutoFixture.AutoMoq;
using backend.Application.Identity.UserProfiles.Commands.SyncUserProfile;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Commands.SyncUserProfile;

public class SyncUserProfileCommandHandlerTests : TestBase
{
    private readonly Mock<ILogger<SyncUserProfileCommandHandler>> _mockLogger;
    private readonly SyncUserProfileCommandHandler _handler;

    public SyncUserProfileCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<SyncUserProfileCommandHandler>>();


        _handler = new SyncUserProfileCommandHandler(
            _context,
            _mockLogger.Object
        );
    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi External ID (ClaimTypes.NameIdentifier) không được tìm thấy trong ClaimsPrincipal của người dùng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một ClaimsPrincipal không chứa ClaimTypes.NameIdentifier.
    ///    - Act: Gọi phương thức Handle của handler với một SyncUserProfileCommand chứa ClaimsPrincipal này.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thất bại (IsSuccess = false) và chứa thông báo lỗi phù hợp ("External ID (sub claim) not found in claims.").
    ///              Xác minh rằng một cảnh báo đã được ghi lại bởi logger.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: External ID là bắt buộc để xác định và đồng bộ hóa hồ sơ người dùng; nếu thiếu, hoạt động không thể tiếp tục.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenExternalIdNotFound()
    {
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Email, "test@example.com"),
            new(ClaimTypes.Name, "Test User")
        }));

        var command = new SyncUserProfileCommand { UserPrincipal = userPrincipal };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("External ID (sub claim) not found in claims.");
        result.ErrorSource.Should().Be("Authentication");
        _mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler tạo một hồ sơ người dùng (UserProfile) và các tùy chọn người dùng (UserPreference) mặc định mới
    /// khi người dùng chưa tồn tại trong hệ thống.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một ClaimsPrincipal hợp lệ với External ID, Email và Name. Đảm bảo cơ sở dữ liệu không chứa UserProfile nào với External ID này.
    ///    - Act: Gọi phương thức Handle của handler với một SyncUserProfileCommand chứa ClaimsPrincipal đã tạo.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thành công (IsSuccess = true) và giá trị là true (newUserCreated).
    ///              Xác minh rằng một UserProfile mới đã được thêm vào cơ sở dữ liệu với các thông tin chính xác.
    ///              Xác minh rằng một UserPreference mặc định (Theme.Light, Language.English) đã được tạo và liên kết với UserProfile mới.
    ///              Xác minh rằng một thông tin đã được ghi lại bởi logger.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Khi một người dùng mới đăng nhập lần đầu, hệ thống phải tự động tạo hồ sơ và thiết lập các tùy chọn mặc định cho họ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCreateNewUserProfileAndPreferences()
    {
        var externalId = Guid.NewGuid().ToString();
        var email = "newuser@example.com";
        var name = "New User";
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.NameIdentifier, externalId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, name)
        }));

        var command = new SyncUserProfileCommand { UserPrincipal = userPrincipal };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue(); // newUserCreated should be true

        _context.UserProfiles.Should().ContainSingle(up => up.ExternalId == externalId);
        var newUserProfile = _context.UserProfiles.First(up => up.ExternalId == externalId);
        newUserProfile.Email.Should().Be(email);
        newUserProfile.Name.Should().Be(name);

        _context.UserPreferences.Should().ContainSingle(up => up.UserProfileId == newUserProfile.Id);
        var newUserPreference = _context.UserPreferences.First(up => up.UserProfileId == newUserProfile.Id);
        newUserPreference.Theme.Should().Be(Theme.Light);
        newUserPreference.Language.Should().Be(Language.English);

        _mockLogger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về thành công và không tạo hồ sơ người dùng hoặc tùy chọn người dùng mới
    /// khi hồ sơ người dùng đã tồn tại trong hệ thống.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile và UserPreference hiện có trong cơ sở dữ liệu.
    ///               Tạo một ClaimsPrincipal với External ID khớp với UserProfile hiện có.
    ///    - Act: Gọi phương thức Handle của handler với một SyncUserProfileCommand chứa ClaimsPrincipal này.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thành công (IsSuccess = true) và giá trị là false (newUserCreated).
    ///              Xác minh rằng không có UserProfile hoặc UserPreference mới nào được thêm vào cơ sở dữ liệu.
    ///              Xác minh rằng không có thông tin nào được ghi lại bởi logger (liên quan đến việc tạo người dùng mới).
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Nếu người dùng đã tồn tại, hệ thống chỉ cần xác nhận sự tồn tại mà không cần tạo lại hồ sơ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnSuccessWhenUserProfileAlreadyExists()
    {
        var externalId = Guid.NewGuid().ToString();
        var existingUserProfile = new UserProfile
        {
            ExternalId = externalId,
            Email = "existing@example.com",
            Name = "Existing User"
        };
        _context.UserProfiles.Add(existingUserProfile);
        _context.UserPreferences.Add(new UserPreference
        {
            UserProfile = existingUserProfile,
            Theme = Theme.Dark,
            Language = Language.Vietnamese
        });
        await _context.SaveChangesAsync();

        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.NameIdentifier, externalId),
            new(ClaimTypes.Email, "existing@example.com"),
            new(ClaimTypes.Name, "Existing User")
        }));

        var command = new SyncUserProfileCommand { UserPrincipal = userPrincipal };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse(); // newUserCreated should be false

        _context.UserProfiles.Count().Should().Be(1); // No new user profile should be added
        _context.UserPreferences.Count().Should().Be(1); // No new user preference should be added
        _mockLogger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }
}
