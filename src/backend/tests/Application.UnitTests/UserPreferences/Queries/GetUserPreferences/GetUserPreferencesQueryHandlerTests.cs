using AutoFixture;
using AutoFixture.Xunit2;
using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.UserPreferences.Queries;
using backend.Application.UserPreferences.Queries.GetUserPreferences;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.UserPreferences.Queries;

public class GetUserPreferencesQueryHandlerTests : TestBase
{
    private readonly GetUserPreferencesQueryHandler _handler;

    public GetUserPreferencesQueryHandlerTests()
    {
        _handler = new GetUserPreferencesQueryHandler(_context, _mockUser.Object, _mapper);
    }

    [Theory, AutoData]
    public async Task Handle_ShouldReturnFailureResult_WhenUserNotAuthenticated(GetUserPreferencesQuery query)
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler trả về lỗi khi người dùng chưa được xác thực (UserId rỗng).

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange: Thiết lập UserId của người dùng hiện tại là rỗng.
        _mockUser.Setup(x => x.Id).Returns((string)null!); // Hoặc string.Empty

        // Act: Gọi handler để xử lý query.
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User is not authenticated.");
        result.ErrorSource.Should().Be("Authentication");

        // 💡 Giải thích: Khi _mockUser.Id là null hoặc rỗng, logic đầu tiên trong handler sẽ bắt lỗi này
        // và trả về Result.Failure với thông báo "User is not authenticated.".
    }

    [Theory, AutoData]
    public async Task Handle_ShouldReturnFailureResult_WhenUserProfileNotFound(GetUserPreferencesQuery query, string userId)
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler trả về lỗi khi không tìm thấy UserProfile cho người dùng đã xác thực.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange: Thiết lập UserId hợp lệ nhưng không có UserProfile nào trong _context.
        _mockUser.Setup(x => x.Id).Returns(userId);
        // Đảm bảo không có UserProfile nào được thêm vào _context cho userId này.

        // Act: Gọi handler để xử lý query.
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
        result.ErrorSource.Should().Be("NotFound");

        // 💡 Giải thích: Sau khi xác thực người dùng, handler sẽ cố gắng tìm UserProfile.
        // Nếu không tìm thấy (do _context.UserProfiles không chứa UserProfile nào với userId đã cho),
        // nó sẽ trả về Result.Failure với thông báo "User profile not found.".
    }

    [Theory, AutoData]
    public async Task Handle_ShouldReturnDefaultPreferences_WhenUserPreferenceNotFound(GetUserPreferencesQuery query, string userId)
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler trả về các cài đặt mặc định khi UserProfile tồn tại
        // nhưng UserPreference liên quan không được tìm thấy.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange: Thiết lập UserId hợp lệ và tạo một UserProfile nhưng không có UserPreference.
        _mockUser.Setup(x => x.Id).Returns(userId);

        var userProfile = _fixture.Build<UserProfile>()
                                 .Without(up => up.UserPreference)
                                 .With(up => up.ExternalId, userId)
                                 .Create();
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync();

        // Act: Gọi handler để xử lý query.
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả trả về là thành công và chứa các giá trị mặc định.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Theme.Should().Be(Theme.Light);
        result.Value!.Language.Should().Be(Language.English);
        result.Value!.EmailNotificationsEnabled.Should().BeTrue();
        result.Value!.SmsNotificationsEnabled.Should().BeFalse();
        result.Value!.InAppNotificationsEnabled.Should().BeTrue();

        // 💡 Giải thích: Handler sẽ tìm thấy UserProfile nhưng UserPreference là null.
        // Trong trường hợp này, handler được thiết kế để trả về một UserPreferenceDto với các giá trị mặc định.
    }

    [Theory, AutoData]
    public async Task Handle_ShouldReturnMappedPreferences_WhenUserPreferenceFound(GetUserPreferencesQuery query, string userId)
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler trả về UserPreference đã được ánh xạ chính xác
        // khi cả UserProfile và UserPreference đều tồn tại.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange: Thiết lập UserId hợp lệ, tạo UserProfile và UserPreference với dữ liệu cụ thể.
        _mockUser.Setup(x => x.Id).Returns(userId);

        var userPreference = _fixture.Build<UserPreference>()
                                     .With(up => up.Theme, Theme.Dark)
                                     .With(up => up.Language, Language.Vietnamese)
                                     .With(up => up.EmailNotificationsEnabled, false)
                                     .With(up => up.SmsNotificationsEnabled, true)
                                     .With(up => up.InAppNotificationsEnabled, false)
                                     .Create();

        var userProfile = _fixture.Build<UserProfile>()
                                 .With(up => up.ExternalId, userId)
                                 .With(up => up.UserPreference, userPreference)
                                 .Create();

        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync();

        // Act: Gọi handler để xử lý query.
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả trả về là thành công và chứa dữ liệu đã được ánh xạ chính xác.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Theme.Should().Be(userPreference.Theme);
        result.Value!.Language.Should().Be(userPreference.Language);
        result.Value!.EmailNotificationsEnabled.Should().Be(userPreference.EmailNotificationsEnabled);
        result.Value!.SmsNotificationsEnabled.Should().Be(userPreference.SmsNotificationsEnabled);
        result.Value!.InAppNotificationsEnabled.Should().Be(userPreference.InAppNotificationsEnabled);

        // 💡 Giải thích: Handler sẽ tìm thấy cả UserProfile và UserPreference.
        // Sau đó, nó sẽ sử dụng IMapper để ánh xạ UserPreference entity sang UserPreferenceDto
        // và trả về kết quả thành công với dữ liệu đã ánh xạ.
    }
}