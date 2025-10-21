using AutoFixture;
using AutoFixture.AutoMoq;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Identity.UserProfiles.Commands.SyncUserProfile;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.UnitTests.Common;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Identity.UserProfiles.Commands.SyncUserProfile;

public class SyncUserProfileCommandHandlerTests : TestBase
{
    private readonly Mock<ILogger<SyncUserProfileCommandHandler>> _mockLogger;
    private readonly SyncUserProfileCommandHandler _handler;

    public SyncUserProfileCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<SyncUserProfileCommandHandler>>();
        _fixture.Customize(new AutoMoqCustomization());

        _handler = new SyncUserProfileCommandHandler(
            _context,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenExternalIdNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi External ID (sub claim) không tìm thấy trong UserPrincipal.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một ClaimsPrincipal không có ClaimTypes.NameIdentifier.
        // 2. Act: Gọi phương thức Handle với SyncUserProfileCommand chứa ClaimsPrincipal đó.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Name, "Test User")
        }));

        var command = new SyncUserProfileCommand { UserPrincipal = userPrincipal };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("External ID (sub claim) not found in claims.");
        result.ErrorSource.Should().Be("Authentication");
        _mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        // 💡 Giải thích: External ID là bắt buộc để đồng bộ hóa hồ sơ người dùng.
    }

    [Fact]
    public async Task Handle_ShouldCreateNewUserProfileAndPreferences()
    {
        // 🎯 Mục tiêu của test: Xác minh handler tạo hồ sơ người dùng và tùy chọn người dùng mới khi người dùng chưa tồn tại.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một ClaimsPrincipal với External ID, Email và Name. Đảm bảo không có UserProfile nào trong Context.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và newUserCreated là true. Xác minh UserProfile và UserPreference mới được thêm vào Context với các giá trị mặc định.
        var externalId = Guid.NewGuid().ToString();
        var email = "newuser@example.com";
        var name = "New User";
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, externalId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name)
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
        // 💡 Giải thích: Khi người dùng mới đăng nhập, một hồ sơ người dùng và tùy chọn mặc định phải được tạo.
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWhenUserProfileAlreadyExists()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về thành công và không tạo hồ sơ mới khi hồ sơ người dùng đã tồn tại.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UserProfile và UserPreference hiện có trong Context. Tạo một ClaimsPrincipal với External ID của UserProfile đó.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và newUserCreated là false. Xác minh không có UserProfile hoặc UserPreference mới nào được thêm vào Context.
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
            new Claim(ClaimTypes.NameIdentifier, externalId),
            new Claim(ClaimTypes.Email, "existing@example.com"),
            new Claim(ClaimTypes.Name, "Existing User")
        }));

        var command = new SyncUserProfileCommand { UserPrincipal = userPrincipal };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse(); // newUserCreated should be false

        _context.UserProfiles.Count().Should().Be(1); // No new user profile should be added
        _context.UserPreferences.Count().Should().Be(1); // No new user preference should be added
        _mockLogger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        // 💡 Giải thích: Nếu hồ sơ người dùng đã tồn tại, handler không nên tạo hồ sơ mới và chỉ trả về thành công.
    }
}
