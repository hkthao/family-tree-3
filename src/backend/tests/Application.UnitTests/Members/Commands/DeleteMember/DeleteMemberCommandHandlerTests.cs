using backend.Application.Members.Commands.DeleteMember;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandlerTests : TestBase
{
    private readonly DeleteMemberCommandHandler _handler;

    public DeleteMemberCommandHandlerTests()
    {
        _handler = new DeleteMemberCommandHandler(
            _context,
            _mockAuthorizationService.Object,
            _mockMediator.Object,
            _mockFamilyTreeService.Object);
    }



    /// <summary>
    /// Kiểm tra xem một thành viên có được xóa thành công khi người dùng có quyền.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một lệnh DeleteMemberCommand hợp lệ được thực thi
    /// bởi một người dùng có quyền quản lý gia đình, thành viên tương ứng sẽ bị xóa khỏi cơ sở dữ liệu.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Delete_Member_Successfully()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData
        var williamId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef"); // Prince William ID from SeedSampleData

        var currentUserProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };
        _context.UserProfiles.Add(currentUserProfile);
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = royalFamilyId, UserProfileId = currentUserProfile.Id, Role = FamilyRole.Manager });
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(royalFamilyId, currentUserProfile)).Returns(true);

        // Thêm một thành viên vào cơ sở dữ liệu để xóa.
        var memberToDelete = new Member { Id = williamId, FirstName = "Thành viên", LastName = "Để Xóa", FamilyId = royalFamilyId, Code = "DM001" };
        _context.Members.Add(memberToDelete);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo lệnh xóa thành viên.
        var command = new DeleteMemberCommand(memberToDelete.Id);

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        var deletedMember = await _context.Members.FindAsync(memberToDelete.Id);
        deletedMember.Should().BeNull();

        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(royalFamilyId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi cố gắng xóa một thành viên không tồn tại.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một lệnh DeleteMemberCommand được thực thi với một ID thành viên không tồn tại,
    /// hệ thống sẽ trả về kết quả thất bại với thông báo lỗi "không tìm thấy".
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_MemberNotFound()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData

        var currentUserProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };
        _context.UserProfiles.Add(currentUserProfile);
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = royalFamilyId, UserProfileId = currentUserProfile.Id, Role = FamilyRole.Manager });
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(royalFamilyId, currentUserProfile)).Returns(true);

        // Tạo lệnh xóa thành viên với một ID không tồn tại.
        var nonExistentMemberId = Guid.NewGuid();
        var command = new DeleteMemberCommand(nonExistentMemberId);

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Member with ID {nonExistentMemberId} not found.");

        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi người dùng không có quyền xóa thành viên.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng chỉ những người dùng có quyền quản lý gia đình mới có thể xóa thành viên.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserIsNotAuthorizedToManageFamily()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData
        var williamId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef"); // Prince William ID from SeedSampleData

        var currentUserProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };
        _context.UserProfiles.Add(currentUserProfile);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(royalFamilyId, currentUserProfile)).Returns(false); // User is not authorized

        // Thêm một thành viên vào cơ sở dữ liệu để xóa.
        var memberToDelete = new Member { Id = williamId, FirstName = "Thành viên", LastName = "Không Được Xóa", FamilyId = royalFamilyId, Code = "DM002" };
        _context.Members.Add(memberToDelete);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo lệnh xóa thành viên.
        var command = new DeleteMemberCommand(memberToDelete.Id);

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied. Only family managers can delete members.");

        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
