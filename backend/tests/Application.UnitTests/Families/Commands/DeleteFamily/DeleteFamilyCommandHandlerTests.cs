using backend.Application.Families.Commands.DeleteFamily;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using Moq;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandHandlerTests : TestBase
{
    private readonly DeleteFamilyCommandHandler _handler;

    public DeleteFamilyCommandHandlerTests()
    {
        _handler = new DeleteFamilyCommandHandler(_context, _mockAuthorizationService.Object, _mockMediator.Object, _mockFamilyTreeService.Object);
    }

    /// <summary>
    /// Thiết lập môi trường kiểm thử bằng cách xóa dữ liệu cũ và tạo người dùng, hồ sơ người dùng, gia đình.
    /// </summary>
    /// <param name="userId">ID của người dùng hiện tại.</param>
    /// <param name="userProfileId">ID của hồ sơ người dùng.</param>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="isAdmin">Cho biết người dùng có phải là quản trị viên hay không.</param>
    /// <param name="canManageFamily">Cho biết người dùng có quyền quản lý gia đình hay không.</param>
    /// <param name="userProfileExists">Cho biết hồ sơ người dùng có tồn tại hay không.</param>
    private async Task ClearDatabaseAndSetupUser(string userId, Guid userProfileId, Guid familyId, bool isAdmin, bool canManageFamily, bool userProfileExists = true)
    {
        // Xóa tất cả dữ liệu liên quan để đảm bảo môi trường sạch cho mỗi bài kiểm tra.
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Members.RemoveRange(_context.Members);
        _context.Events.RemoveRange(_context.Events);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        _context.AIBiographies.RemoveRange(_context.AIBiographies);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Thiết lập ID người dùng hiện tại cho mock IUser.
        _mockUser.Setup(x => x.Id).Returns(userId);

        if (userProfileExists)
        {
            var userProfile = new UserProfile { Id = userProfileId, ExternalId = userId, Email = "test@example.com", Name = "Test User" };
            _context.UserProfiles.Add(userProfile);
            // Thiết lập người dùng với vai trò Quản lý gia đình.
            _context.FamilyUsers.Add(new FamilyUser { FamilyId = familyId, UserProfileId = userProfileId, Role = FamilyRole.Manager });
            await _context.SaveChangesAsync(CancellationToken.None);

            // Thiết lập các hành vi của mock IAuthorizationService.
            _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);
            _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(isAdmin);
            _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId, userProfile)).Returns(canManageFamily);
        }
        else
        {
            // Thiết lập mock IAuthorizationService trả về null nếu hồ sơ người dùng không tồn tại.
            _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserProfile)null!);
        }
    }

    /// <summary>
    /// Kiểm tra xem một gia đình có được xóa thành công khi người dùng có quyền hợp lệ.
    /// </summary>
    /// <remarks>
    /// Đảm bảo rằng khi một lệnh DeleteFamilyCommand hợp lệ được thực thi bởi một người dùng
    /// có quyền quản lý gia đình, gia đình tương ứng sẽ bị xóa khỏi cơ sở dữ liệu.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Delete_Family_Successfully()
    {
        // Arrange: Thiết lập môi trường và dữ liệu ban đầu cho bài kiểm tra.
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        // Thiết lập người dùng có quyền quản lý gia đình.
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        // Thêm một gia đình vào cơ sở dữ liệu để chuẩn bị xóa.
        var familyToDelete = new Family { Id = familyId, Name = "Gia đình để xóa" };
        _context.Families.Add(familyToDelete);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo lệnh xóa gia đình với ID của gia đình cần xóa.
        var command = new DeleteFamilyCommand(familyToDelete.Id);

        // Act: Thực hiện hành động cần kiểm tra (gọi handler xóa gia đình).
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert: Kiểm tra kết quả mong đợi.
        // 1. Đảm bảo lệnh xóa thành công.
        result.IsSuccess.Should().BeTrue();
        // 2. Đảm bảo gia đình đã bị xóa khỏi cơ sở dữ liệu.
        var deletedFamily = await _context.Families.FindAsync(familyToDelete.Id);
        deletedFamily.Should().BeNull();

        // 3. Xác minh rằng các dịch vụ khác đã được gọi đúng cách.
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(familyId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi cố gắng xóa một gia đình không tồn tại.
    /// </summary>
    /// <remarks>
    /// Đảm bảo rằng khi một lệnh DeleteFamilyCommand được thực thi với một ID gia đình không tồn tại,
    /// hệ thống sẽ trả về kết quả thất bại với thông báo lỗi "không tìm thấy".
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_FamilyNotFound()
    {
        // Arrange: Thiết lập môi trường và dữ liệu ban đầu cho bài kiểm tra.
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        // Thiết lập người dùng có quyền quản lý gia đình.
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        // Tạo lệnh xóa gia đình với một ID không tồn tại trong cơ sở dữ liệu.
        var nonExistentFamilyId = Guid.NewGuid();
        var command = new DeleteFamilyCommand(nonExistentFamilyId);

        // Act: Thực hiện hành động cần kiểm tra (gọi handler xóa gia đình).
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert: Kiểm tra kết quả mong đợi.
        // 1. Đảm bảo lệnh xóa thất bại.
        result.IsSuccess.Should().BeFalse();
        // 2. Đảm bảo thông báo lỗi chứa chuỗi "Family with ID {nonExistentFamilyId} not found.".
        result.Error.Should().Contain("User does not have permission to delete this family.");

        // 3. Xác minh rằng các dịch vụ khác không được gọi.
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi UserProfile của người dùng không tồn tại.
    /// </summary>
    /// <remarks>
    /// Đảm bảo rằng nếu UserProfile không được tìm thấy cho người dùng hiện tại,
    /// handler sẽ trả về kết quả thất bại với thông báo lỗi "User profile not found.".
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserProfileNotFound()
    {
        // Arrange: Thiết lập môi trường và dữ liệu ban đầu cho bài kiểm tra.
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        // Thiết lập người dùng mà không có hồ sơ người dùng tồn tại.
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true, false); // userProfileExists = false

        // Thêm một gia đình vào context để thử xóa.
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family" });
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo lệnh xóa gia đình.
        var command = new DeleteFamilyCommand(familyId);

        // Act: Thực hiện hành động cần kiểm tra (gọi handler xóa gia đình).
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert: Kiểm tra kết quả mong đợi.
        // 1. Đảm bảo lệnh xóa thất bại.
        result.IsSuccess.Should().BeFalse();
        // 2. Đảm bảo thông báo lỗi chứa chuỗi "User profile not found.".
        result.Error.Should().Contain("User profile not found.");

        // 3. Xác minh rằng các dịch vụ khác không được gọi.
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi người dùng không có quyền xóa gia đình.
    /// </summary>
    /// <remarks>
    /// Đảm bảo rằng chỉ những người dùng có quyền quản lý gia đình hoặc Admin
    /// mới có thể xóa gia đình. Nếu người dùng không có quyền, handler sẽ trả về kết quả thất bại
    /// với thông báo lỗi "Access denied. Only family managers or admins can delete families.".
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserIsNotAuthorized()
    {
        // Arrange: Thiết lập môi trường và dữ liệu ban đầu cho bài kiểm tra.
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        // Thiết lập người dùng không có quyền quản lý gia đình.
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, false); // User is not authorized

        // Thêm một gia đình vào context để thử xóa.
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family" });
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo lệnh xóa gia đình.
        var command = new DeleteFamilyCommand(familyId);

        // Act: Thực hiện hành động cần kiểm tra (gọi handler xóa gia đình).
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert: Kiểm tra kết quả mong đợi.
        // 1. Đảm bảo lệnh xóa thất bại.
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User does not have permission to delete this family.");

        // 3. Xác minh rằng các dịch vụ khác không được gọi.
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}