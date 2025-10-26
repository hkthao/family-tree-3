using AutoFixture.AutoMoq;
using backend.Application.Common.Constants;
using backend.Application.Identity.UserProfiles.Queries.GetCurrentUserProfile;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Queries.GetCurrentUserProfile;

public class GetCurrentUserProfileQueryHandlerTests : TestBase
{
    private readonly GetCurrentUserProfileQueryHandler _handler;

    public GetCurrentUserProfileQueryHandlerTests()
    {
        _fixture.Customize(new AutoMoqCustomization());

        _handler = new GetCurrentUserProfileQueryHandler(
            _context,
            _mockUser.Object,
            _mapper
        );
    }


    [Fact]
    public async Task Handle_ShouldReturnFailureWhenUserProfileNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy hồ sơ người dùng cho người dùng hiện tại.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockUser.Id trả về một ID hợp lệ. Đảm bảo không có UserProfile nào trong Context khớp với ID đó.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);

        var query = new GetCurrentUserProfileQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
        result.ErrorSource.Should().Be("NotFound");
        // 💡 Giải thích: Hồ sơ người dùng phải tồn tại để có thể truy xuất.
    }

    [Fact]
    public async Task Handle_ShouldReturnCurrentUserProfileSuccessfully()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về hồ sơ người dùng hiện tại thành công.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockUser.Id trả về một ID hợp lệ. Thêm một UserProfile vào Context khớp với ID đó.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa UserProfileDto khớp với hồ sơ người dùng đã thêm.
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);

        var existingUserProfile = new UserProfile
        {
            ExternalId = userId.ToString(),
            Email = "current@example.com",
            Name = "Current User"
        };
        _context.UserProfiles.Add(existingUserProfile);
        await _context.SaveChangesAsync();

        var query = new GetCurrentUserProfileQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ExternalId.Should().Be(userId.ToString());
        result.Value.Email.Should().Be("current@example.com");
        result.Value.Name.Should().Be("Current User");
        // 💡 Giải thích: Handler phải truy xuất và ánh xạ đúng hồ sơ người dùng hiện tại.
    }

    [Fact]
    public async Task Handle_ShouldIncludeRolesInUserProfileDto()
    {
        // 🎯 Mục tiêu của test: Xác minh handler bao gồm các vai trò của người dùng trong UserProfileDto.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockUser.Id trả về một ID hợp lệ và _mockUser.Roles trả về một danh sách vai trò.
        //             Thêm một UserProfile vào Context khớp với ID đó.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và UserProfileDto chứa các vai trò đã thiết lập.
        var userId = Guid.NewGuid();
        var roles = new List<string> { "Admin", "User" };
        _mockUser.Setup(u => u.Id).Returns(userId);
        _mockUser.Setup(u => u.Roles).Returns(roles);

        var existingUserProfile = new UserProfile
        {
            ExternalId = userId.ToString(),
            Email = "current@example.com",
            Name = "Current User"
        };
        _context.UserProfiles.Add(existingUserProfile);
        await _context.SaveChangesAsync();

        var query = new GetCurrentUserProfileQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Roles.Should().BeEquivalentTo(roles);
        // 💡 Giải thích: UserProfileDto phải chứa các vai trò của người dùng nếu chúng được cung cấp bởi dịch vụ người dùng.
    }
}
