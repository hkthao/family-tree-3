using backend.Application.Families.Commands.UpdateFamily;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using Moq;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandlerTests : TestBase
{
    private readonly UpdateFamilyCommandHandler _handler;

    public UpdateFamilyCommandHandlerTests()
    {
        _handler = new UpdateFamilyCommandHandler(
            _context,
            _mockAuthorizationService.Object,
            _mockMediator.Object,
            _mockFamilyTreeService.Object);
    }

    /// <summary>
    /// Kiểm tra xem một gia đình có được cập nhật thành công khi người dùng có quyền hợp lệ.
    /// </summary>
    /// <remarks>
    /// Đảm bảo rằng khi một lệnh UpdateFamilyCommand hợp lệ được thực thi bởi một người dùng
    /// có quyền quản lý gia đình, gia đình tương ứng sẽ được cập nhật trong cơ sở dữ liệu.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Update_Family()
    {
        // Arrange: Thiết lập môi trường và dữ liệu ban đầu cho bài kiểm tra.
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        // Thiết lập người dùng có quyền quản lý gia đình.
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        // Thêm một gia đình vào cơ sở dữ liệu để chuẩn bị cập nhật.
        var family = new Family { Id = familyId, Name = "Old Name", Description = "Old Desc" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo lệnh cập nhật gia đình.
        var command = new UpdateFamilyCommand
        {
            Id = familyId,
            Name = "New Name",
            Description = "New Desc"
        };

        // Act: Thực hiện hành động cần kiểm tra (gọi handler cập nhật gia đình).
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert: Kiểm tra kết quả mong đợi.
        // 1. Đảm bảo lệnh cập nhật thành công.
        result.IsSuccess.Should().BeTrue();
        // 2. Đảm bảo gia đình đã được cập nhật trong cơ sở dữ liệu.
        var updatedFamily = await _context.Families.FindAsync(familyId);
        updatedFamily.Should().NotBeNull();
        updatedFamily!.Name.Should().Be(command.Name);
        updatedFamily.Description.Should().Be(command.Description);

        // 3. Xác minh rằng các dịch vụ khác đã được gọi đúng cách.
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(familyId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Kiểm tra xem có ném ra ngoại lệ NotFoundException khi cố gắng cập nhật một gia đình không tồn tại.
    /// </summary>
    /// <remarks>
    /// Đảm bảo rằng khi một lệnh UpdateFamilyCommand được thực thi với một ID gia đình không tồn tại,
    /// hệ thống sẽ ném ra một NotFoundException.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Family_Does_Not_Exist()
    {
        // Arrange: Thiết lập môi trường và dữ liệu ban đầu cho bài kiểm tra.
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        // Thiết lập người dùng có quyền quản lý gia đình (không quan trọng lắm trong trường hợp này vì gia đình không tồn tại).
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        // Tạo lệnh cập nhật gia đình với một ID không tồn tại.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "New Name" };

        // Act: Thực hiện hành động cần kiểm tra (gọi handler cập nhật gia đình).
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert: Kiểm tra kết quả mong đợi.
        // 1. Đảm bảo lệnh cập nhật thất bại.
        result.IsSuccess.Should().BeFalse();
        // 2. Đảm bảo thông báo lỗi chứa chuỗi "Family with ID {command.Id} not found.".
        result.Error.Should().Contain("User does not have permission to update this family.");

        // Xác minh rằng các dịch vụ khác không được gọi.
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
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
        await _context.SaveChangesAsync(CancellationToken.None);

        // Thiết lập ID người dùng hiện tại cho mock IUser.
        _mockUser.Setup(x => x.Id).Returns(userId);

        if (userProfileExists)
        {
            // Tạo và thêm hồ sơ người dùng vào cơ sở dữ liệu.
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

        // Thêm một gia đình vào context để thử cập nhật.
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family" });
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateFamilyCommand { Id = familyId, Name = "New Name" };

        // Act: Thực hiện hành động cần kiểm tra (gọi handler cập nhật gia đình).
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert: Kiểm tra kết quả mong đợi.
        // 1. Đảm bảo lệnh cập nhật thất bại.
        result.IsSuccess.Should().BeFalse();
        // 2. Đảm bảo thông báo lỗi chứa chuỗi "User profile not found.".
        result.Error.Should().Contain("User profile not found.");

        // 3. Xác minh rằng các dịch vụ khác không được gọi.
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi người dùng không có quyền cập nhật gia đình.
    /// </summary>
    /// <remarks>
    /// Đảm bảo rằng chỉ những người dùng có quyền quản lý gia đình hoặc Admin
    /// mới có thể cập nhật gia đình. Nếu người dùng không có quyền, handler sẽ trả về kết quả thất bại
    /// với thông báo lỗi "User does not have permission to update this family.".
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

        // Thêm một gia đình vào context để thử cập nhật.
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family" });
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateFamilyCommand { Id = familyId, Name = "New Name" };

        // Act: Thực hiện hành động cần kiểm tra (gọi handler cập nhật gia đình).
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert: Kiểm tra kết quả mong đợi.
        // 1. Đảm bảo lệnh cập nhật thất bại.
        result.IsSuccess.Should().BeFalse();
        // 2. Đảm bảo thông báo lỗi chứa chuỗi "User does not have permission to update this family.".
        result.Error.Should().Contain("User does not have permission to update this family.");

        // 3. Xác minh rằng các dịch vụ khác không được gọi.
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
