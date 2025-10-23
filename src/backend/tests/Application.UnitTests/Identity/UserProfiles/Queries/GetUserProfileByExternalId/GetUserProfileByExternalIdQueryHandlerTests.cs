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
        _fixture.Customize(new AutoMoqCustomization());

        _handler = new GetUserProfileByExternalIdQueryHandler(
            _context,
            _mapper
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenUserProfileNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy hồ sơ người dùng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một GetUserProfileByExternalIdQuery với ExternalId không tồn tại trong DB.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var query = new GetUserProfileByExternalIdQuery { ExternalId = "nonexistent_external_id" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
        result.ErrorSource.Should().Be("NotFound");
        // 💡 Giải thích: Hồ sơ người dùng phải tồn tại để có thể truy xuất.
    }

    [Fact]
    public async Task Handle_ShouldReturnUserProfileByExternalIdSuccessfully()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về hồ sơ người dùng theo ExternalId thành công.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm một UserProfile vào Context với một ExternalId cụ thể.
        // 2. Act: Gọi phương thức Handle với GetUserProfileByExternalIdQuery chứa ExternalId đó.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa UserProfileDto khớp với hồ sơ người dùng đã thêm.
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
        // 💡 Giải thích: Handler phải truy xuất và ánh xạ đúng hồ sơ người dùng dựa trên ExternalId.
    }
}
