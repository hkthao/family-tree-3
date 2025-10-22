using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Infrastructure.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Infrastructure.IntegrationTests.Services;

[Collection(nameof(IntegrationTestCollection))]
public class AuthorizationServiceTests : IntegrationTestBase
{
    private readonly Mock<IUser> _mockUser;
    private readonly AuthorizationService _service;

    public AuthorizationServiceTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _mockUser = new Mock<IUser>();
        _service = new AuthorizationService(_mockUser.Object, _dbContext);
    }

    [Fact]
    public void IsAdmin_ShouldReturnTrue_WhenUserHasAdminRole()
    {
        // 🎯 Mục tiêu: Xác minh IsAdmin trả về true khi người dùng có vai trò "Admin".

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thiết lập _mockUser.Setup(u => u.Roles).Returns(new List<string> { "Admin" });.
        _mockUser.Setup(u => u.Roles).Returns(new List<string> { "Admin" });

        // 2. Act: Gọi IsAdmin().
        var result = _service.IsAdmin();

        // 3. Assert: Kiểm tra rằng kết quả là true.
        result.Should().BeTrue();
        // 💡 Giải thích: Dịch vụ phải xác định đúng vai trò quản trị viên của người dùng.
    }

    [Fact]
    public void IsAdmin_ShouldReturnFalse_WhenUserDoesNotHaveAdminRole()
    {
        // 🎯 Mục tiêu: Xác minh IsAdmin trả về false khi người dùng không có vai trò "Admin".

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thiết lập _mockUser.Setup(u => u.Roles).Returns(new List<string> { "User" });.
        _mockUser.Setup(u => u.Roles).Returns(new List<string> { "User" });

        // 2. Act: Gọi IsAdmin().
        var result = _service.IsAdmin();

        // 3. Assert: Kiểm tra rằng kết quả là false.
        result.Should().BeFalse();
        // 💡 Giải thích: Dịch vụ phải xác định đúng vai trò quản trị viên của người dùng.
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_ShouldReturnProfile_WhenUserAuthenticatedAndProfileExists()
    {
        // 🎯 Mục tiêu: Xác minh GetCurrentUserProfileAsync trả về hồ sơ người dùng khi người dùng được xác thực và hồ sơ tồn tại.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một UserProfile với ExternalId khớp với _user.Id. Thêm UserProfile vào DB.
        var externalId = Guid.NewGuid().ToString();
        var expectedProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = externalId, Email = "test@example.com", Name = "Test User" };
        _dbContext.UserProfiles.Add(expectedProfile);
        await _dbContext.SaveChangesAsync();

        _mockUser.Setup(u => u.Id).Returns(externalId);

        // 2. Act: Gọi GetCurrentUserProfileAsync.
        var result = await _service.GetCurrentUserProfileAsync();

        // 3. Assert: Kiểm tra rằng kết quả trả về là UserProfile chính xác.
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedProfile.Id);
        result.ExternalId.Should().Be(expectedProfile.ExternalId);
        // 💡 Giải thích: Dịch vụ phải truy xuất hồ sơ người dùng dựa trên ID người dùng được xác thực.
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_ShouldReturnNull_WhenUserNotAuthenticated()
    {
        // 🎯 Mục tiêu: Xác minh GetCurrentUserProfileAsync trả về null khi người dùng không được xác thực.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thiết lập _mockUser.Setup(u => u.Id).Returns(string.Empty);.
        _mockUser.Setup(u => u.Id).Returns(string.Empty);

        // 2. Act: Gọi GetCurrentUserProfileAsync.
        var result = await _service.GetCurrentUserProfileAsync();

        // 3. Assert: Kiểm tra rằng kết quả là null.
        result.Should().BeNull();
        // 💡 Giải thích: Dịch vụ phải trả về null khi không có người dùng nào được xác thực.
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_ShouldReturnNull_WhenProfileDoesNotExist()
    {
        // 🎯 Mục tiêu: Xác minh GetCurrentUserProfileAsync trả về null khi hồ sơ người dùng không tồn tại trong DB.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thiết lập _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());. Đảm bảo không có UserProfile nào trong DB khớp với _user.Id.
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());

        // 2. Act: Gọi GetCurrentUserProfileAsync.
        var result = await _service.GetCurrentUserProfileAsync();

        // 3. Assert: Kiểm tra rằng kết quả là null.
        result.Should().BeNull();
        // 💡 Giải thích: Dịch vụ phải trả về null khi không tìm thấy hồ sơ người dùng trong cơ sở dữ liệu.
    }

    [Fact]
    public void CanAccessFamily_ShouldReturnTrue_WhenUserIsMemberOfFamily()
    {
        // 🎯 Mục tiêu: Xác minh CanAccessFamily trả về true khi người dùng là thành viên của gia đình.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một Family và một UserProfile. Tạo một FamilyUser liên kết UserProfile với Family đó.
        var familyId = Guid.NewGuid();
        var userProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };
        userProfile.FamilyUsers.Add(new FamilyUser { FamilyId = familyId, UserProfileId = userProfile.Id, Role = FamilyRole.Viewer });

        // 2. Act: Gọi CanAccessFamily với FamilyId và UserProfile.
        var result = _service.CanAccessFamily(familyId, userProfile);

        // 3. Assert: Kiểm tra rằng kết quả là true.
        result.Should().BeTrue();
        // 💡 Giải thích: Người dùng có quyền truy cập nếu họ là thành viên của gia đình.
    }

    [Fact]
    public void CanAccessFamily_ShouldReturnFalse_WhenUserIsNotMemberOfFamily()
    {
        // 🎯 Mục tiêu: Xác minh CanAccessFamily trả về false khi người dùng không phải là thành viên của gia đình.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một Family và một UserProfile. Đảm bảo UserProfile.FamilyUsers không chứa FamilyUser liên kết với Family đó.
        var familyId = Guid.NewGuid();
        var userProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };
        // Không thêm FamilyUser cho familyId này

        // 2. Act: Gọi CanAccessFamily với FamilyId và UserProfile.
        var result = _service.CanAccessFamily(familyId, userProfile);

        // 3. Assert: Kiểm tra rằng kết quả là false.
        result.Should().BeFalse();
        // 💡 Giải thích: Người dùng không có quyền truy cập nếu họ không phải là thành viên của gia đình.
    }

    [Fact]
    public void CanManageFamily_ShouldReturnTrue_WhenUserIsManagerOfFamily()
    {
        // 🎯 Mục tiêu: Xác minh CanManageFamily trả về true khi người dùng là người quản lý của gia đình.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một Family và một UserProfile. Tạo một FamilyUser liên kết UserProfile với Family đó và Role = FamilyRole.Manager.
        var familyId = Guid.NewGuid();
        var userProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };
        userProfile.FamilyUsers.Add(new FamilyUser { FamilyId = familyId, UserProfileId = userProfile.Id, Role = FamilyRole.Manager });

        // 2. Act: Gọi CanManageFamily với FamilyId và UserProfile.
        var result = _service.CanManageFamily(familyId, userProfile);

        // 3. Assert: Kiểm tra rằng kết quả là true.
        result.Should().BeTrue();
        // 💡 Giải thích: Người dùng có quyền quản lý nếu họ có vai trò Manager trong gia đình.
    }

    [Fact]
    public void CanManageFamily_ShouldReturnFalse_WhenUserIsNotManagerOfFamily()
    {
        // 🎯 Mục tiêu: Xác minh CanManageFamily trả về false khi người dùng không phải là người quản lý của gia đình.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một Family và một UserProfile. Tạo một FamilyUser liên kết UserProfile với Family đó và Role = FamilyRole.Viewer.
        var familyId = Guid.NewGuid();
        var userProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };
        userProfile.FamilyUsers.Add(new FamilyUser { FamilyId = familyId, UserProfileId = userProfile.Id, Role = FamilyRole.Viewer });

        // 2. Act: Gọi CanManageFamily với FamilyId và UserProfile.
        var result = _service.CanManageFamily(familyId, userProfile);

        // 3. Assert: Kiểm tra rằng kết quả là false.
        result.Should().BeFalse();
        // 💡 Giải thích: Người dùng không có quyền quản lý nếu họ không có vai trò Manager trong gia đình.
    }

    [Fact]
    public void HasFamilyRole_ShouldReturnTrue_WhenUserHasRequiredRole()
    {
        // 🎯 Mục tiêu: Xác minh HasFamilyRole trả về true khi người dùng có vai trò yêu cầu.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một Family và một UserProfile. Tạo một FamilyUser liên kết UserProfile với Family đó và Role = FamilyRole.Manager.
        var familyId = Guid.NewGuid();
        var userProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };
        userProfile.FamilyUsers.Add(new FamilyUser { FamilyId = familyId, UserProfileId = userProfile.Id, Role = FamilyRole.Manager });

        // 2. Act: Gọi HasFamilyRole với FamilyId, UserProfile và FamilyRole.Viewer.
        var result = _service.HasFamilyRole(familyId, userProfile, FamilyRole.Viewer);

        // 3. Assert: Kiểm tra rằng kết quả là true.
        result.Should().BeTrue();
        // 💡 Giải thích: Người dùng có vai trò Manager, cao hơn hoặc bằng Viewer, nên có quyền.
    }

    [Fact]
    public void HasFamilyRole_ShouldReturnFalse_WhenUserDoesNotHaveRequiredRole()
    {
        // 🎯 Mục tiêu: Xác minh HasFamilyRole trả về false khi người dùng không có vai trò yêu cầu.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một Family và một UserProfile. Tạo một FamilyUser liên kết UserProfile với Family đó và Role = FamilyRole.Viewer.
        var familyId = Guid.NewGuid();
        var userProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };
        userProfile.FamilyUsers.Add(new FamilyUser { FamilyId = familyId, UserProfileId = userProfile.Id, Role = FamilyRole.Viewer });

        // 2. Act: Gọi HasFamilyRole với FamilyId, UserProfile và FamilyRole.Manager.
        var result = _service.HasFamilyRole(familyId, userProfile, FamilyRole.Manager);

        // 3. Assert: Kiểm tra rằng kết quả là false.
        result.Should().BeFalse();
        // 💡 Giải thích: Người dùng có vai trò Viewer, thấp hơn Manager, nên không có quyền.
    }

    [Fact]
    public void HasFamilyRole_ShouldReturnTrue_WhenUserIsAdmin()
    {
        // 🎯 Mục tiêu: Xác minh HasFamilyRole trả về true khi người dùng là Admin, bất kể vai trò gia đình.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thiết lập _mockUser.Setup(u => u.Roles).Returns(new List<string> { "Admin" });.
        _mockUser.Setup(u => u.Roles).Returns(new List<string> { "Admin" });
        var familyId = Guid.NewGuid();
        var userProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };

        // 2. Act: Gọi HasFamilyRole với FamilyId, UserProfile và bất kỳ FamilyRole nào.
        var result = _service.HasFamilyRole(familyId, userProfile, FamilyRole.Viewer);

        // 3. Assert: Kiểm tra rằng kết quả là true.
        result.Should().BeTrue();
        // 💡 Giải thích: Người dùng Admin luôn có quyền, bất kể vai trò cụ thể trong gia đình.
    }
}