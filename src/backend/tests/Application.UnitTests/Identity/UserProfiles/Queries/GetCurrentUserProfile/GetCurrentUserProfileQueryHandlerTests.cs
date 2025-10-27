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
        _handler = new GetCurrentUserProfileQueryHandler(
            _context,
            _mockUser.Object,
            _mapper
        );
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi không tìm thấy hồ sơ người dùng (UserProfile) cho người dùng hiện tại.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockUser.Id để trả về một ID hợp lệ. Đảm bảo không có UserProfile nào trong cơ sở dữ liệu khớp với ID đó.
    ///    - Act: Gọi phương thức Handle của handler với một GetCurrentUserProfileQuery.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thất bại (IsSuccess = false) và chứa thông báo lỗi phù hợp ("User profile not found.").
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Hồ sơ người dùng phải tồn tại để có thể được truy xuất; nếu không, hệ thống phải báo cáo lỗi.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenUserProfileNotFound()
    {
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);

        var query = new GetCurrentUserProfileQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
        result.ErrorSource.Should().Be("NotFound");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về hồ sơ người dùng hiện tại thành công
    /// khi hồ sơ người dùng tồn tại trong cơ sở dữ liệu.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockUser.Id để trả về một ID hợp lệ. Thêm một UserProfile vào cơ sở dữ liệu khớp với ID đó.
    ///    - Act: Gọi phương thức Handle của handler với một GetCurrentUserProfileQuery.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thành công (IsSuccess = true) và chứa UserProfileDto khớp với hồ sơ người dùng đã thêm.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải truy xuất và ánh xạ đúng hồ sơ người dùng hiện tại từ cơ sở dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnCurrentUserProfileSuccessfully()
    {
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);

        var existingUserProfile = new UserProfile
        {
            Id = userId, // Explicitly set Id to match _mockUser.Id
            ExternalId = Guid.NewGuid().ToString(), // ExternalId can be different
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
        result.Value!.ExternalId.Should().Be(existingUserProfile.ExternalId);
        result.Value.Email.Should().Be("current@example.com");
        result.Value.Name.Should().Be("Current User");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler bao gồm các vai trò của người dùng trong UserProfileDto
    /// khi các vai trò được cung cấp bởi dịch vụ người dùng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockUser.Id để trả về một ID hợp lệ và _mockUser.Roles để trả về một danh sách các vai trò.
    ///               Thêm một UserProfile vào cơ sở dữ liệu khớp với ID đó.
    ///    - Act: Gọi phương thức Handle của handler với một GetCurrentUserProfileQuery.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thành công (IsSuccess = true) và UserProfileDto chứa các vai trò đã thiết lập.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: UserProfileDto phải phản ánh đầy đủ thông tin về người dùng, bao gồm cả các vai trò của họ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldIncludeRolesInUserProfileDto()
    {
        var userId = Guid.NewGuid();
        var roles = new List<string> { "Admin", "User" };
        _mockUser.Setup(u => u.Id).Returns(userId);
        _mockUser.Setup(u => u.Roles).Returns(roles);

        var existingUserProfile = new UserProfile
        {
            Id = userId, // Explicitly set Id to match _mockUser.Id
            ExternalId = Guid.NewGuid().ToString(), // ExternalId can be different
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
    }
}