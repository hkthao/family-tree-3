using backend.Application.Common.Constants;
using AutoFixture.AutoMoq;
using backend.Application.Identity.UserProfiles.Queries.GetUserProfileByExternalId;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Queries.GetUserProfileByExternalId;

public class GetUserProfileByExternalIdQueryHandlerTests : TestBase
{
    private readonly GetUserProfileByExternalIdQueryHandler _handler;

    public GetUserProfileByExternalIdQueryHandlerTests()
    {
        _handler = new GetUserProfileByExternalIdQueryHandler(
            _context,
            _mapper
        );
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi không tìm thấy hồ sơ người dùng (UserProfile) với ExternalId được cung cấp.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GetUserProfileByExternalIdQuery với một ExternalId không tồn tại trong cơ sở dữ liệu.
    ///    - Act: Gọi phương thức Handle của handler với query đã tạo.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thất bại (IsSuccess = false) và chứa thông báo lỗi phù hợp ("User profile not found.").
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Hệ thống không thể truy xuất hồ sơ người dùng nếu ExternalId không khớp với bất kỳ hồ sơ nào hiện có.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenUserProfileNotFound()
    {
        var query = new GetUserProfileByExternalIdQuery { ExternalId = "nonexistent_external_id" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(ErrorMessages.UserProfileNotFound);
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về hồ sơ người dùng thành công
    /// khi một UserProfile với ExternalId được cung cấp tồn tại trong cơ sở dữ liệu.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một UserProfile vào cơ sở dữ liệu với một ExternalId cụ thể. 
    ///    - Act: Gọi phương thức Handle của handler với một GetUserProfileByExternalIdQuery chứa ExternalId đó.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thành công (IsSuccess = true) và chứa UserProfileDto khớp với hồ sơ người dùng đã thêm.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải truy xuất và ánh xạ đúng hồ sơ người dùng dựa trên ExternalId được cung cấp.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnUserProfileByExternalIdSuccessfully()
    {
        var externalId = Guid.NewGuid().ToString();
        var existingUserProfile = new UserProfile
        {
            ExternalId = externalId,
            Email = "test@example.com",
            Name = "Test User"
        };
        _context.UserProfiles.Add(existingUserProfile);
        await _context.SaveChangesAsync();

        var query = new GetUserProfileByExternalIdQuery { ExternalId = externalId };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ExternalId.Should().Be(externalId);
        result.Value.Email.Should().Be("test@example.com");
        result.Value.Name.Should().Be("Test User");
    }
}