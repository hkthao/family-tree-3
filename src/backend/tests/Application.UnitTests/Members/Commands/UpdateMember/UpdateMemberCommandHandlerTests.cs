using backend.Application.Common.Exceptions;
using backend.Application.Members.Commands.UpdateMember;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandlerTests : TestBase
{
    private readonly UpdateMemberCommandHandler _handler;
    public UpdateMemberCommandHandlerTests()
    {
        _handler = new UpdateMemberCommandHandler(_context, _mockAuthorizationService.Object, _mockMediator.Object, _mockFamilyTreeService.Object);
    }

    private async Task ClearDatabaseAndSetupUser(string userId, Guid userProfileId, Guid familyId, bool isAdmin, bool canManageFamily, bool userProfileExists = true)
    {
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Members.RemoveRange(_context.Members);
        _context.Events.RemoveRange(_context.Events);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        if (userProfileExists)
        {
            var userProfile = new UserProfile { Id = userProfileId, ExternalId = userId, Email = "test@example.com", Name = "Test User" };
            _context.UserProfiles.Add(userProfile);
            _context.Families.Add(new Family { Id = familyId, Name = "Test Family" });
            _context.FamilyUsers.Add(new FamilyUser { FamilyId = familyId, UserProfileId = userProfileId, Role = FamilyRole.Manager });
            await _context.SaveChangesAsync(CancellationToken.None);

            _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);
            _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(isAdmin);
            _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId, userProfile)).Returns(canManageFamily);
        }
        else
        {
            _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserProfile)null!);
        }
    }

    /// <summary>
    /// Kiểm tra xem một thành viên có được cập nhật thành công khi tất cả các điều kiện hợp lệ.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một lệnh UpdateMemberCommand hợp lệ được thực thi
    /// bởi một người dùng có quyền quản lý gia đình, thành viên tương ứng sẽ được cập nhật trong cơ sở dữ liệu.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Update_Member_Successfully()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        // Thêm một thành viên vào cơ sở dữ liệu để cập nhật.
        var memberToUpdate = new Member { Id = Guid.NewGuid(), FirstName = "Tên Cũ", LastName = "Họ Cũ", FamilyId = familyId };
        _context.Members.Add(memberToUpdate);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateMemberCommand
        {
            Id = memberToUpdate.Id,
            FirstName = "Tên Mới",
            LastName = "Họ Mới",
            Gender = "Female",
            FamilyId = familyId
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        var updatedMember = await _context.Members.FindAsync(memberToUpdate.Id);
        updatedMember.Should().NotBeNull();
        updatedMember!.FirstName.Should().Be("Tên Mới");
        updatedMember.LastName.Should().Be("Họ Mới");

        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(familyId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Kiểm tra xem có ném ra ngoại lệ NotFoundException khi cố gắng cập nhật một thành viên không tồn tại.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một lệnh UpdateMemberCommand được thực thi với một ID thành viên không tồn tại,
    /// hệ thống sẽ ném ra một NotFoundException.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_MemberNotFound()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        var invalidId = Guid.NewGuid();
        var command = new UpdateMemberCommand { Id = invalidId, FirstName = "Tên", LastName = "Họ", FamilyId = familyId };

        // Act (Thực hiện hành động cần kiểm tra)
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        await act.Should().ThrowAsync<NotFoundException>();

        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi người dùng không có quyền cập nhật thành viên.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng chỉ những người dùng có quyền quản lý gia đình mới có thể cập nhật thành viên.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserIsNotAuthorizedToManageFamily()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, false); // User is not authorized

        // Thêm một thành viên vào cơ sở dữ liệu để cập nhật.
        var memberToUpdate = new Member { Id = Guid.NewGuid(), FirstName = "Tên", LastName = "Họ", FamilyId = familyId };
        _context.Members.Add(memberToUpdate);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateMemberCommand
        {
            Id = memberToUpdate.Id,
            FirstName = "Tên Mới",
            LastName = "Họ Mới",
            FamilyId = familyId
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied. Only family managers can update members.");

        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Kiểm tra logic khi cập nhật một thành viên thành gốc mới (IsRoot = true).
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một thành viên hiện có được cập nhật với IsRoot = true,
    /// nếu đã có một thành viên gốc khác trong cùng gia đình, thành viên gốc cũ sẽ được đặt lại IsRoot = false.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_SetOldRootToNonRoot_WhenNewRootIsUpdated()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        // Tạo một thành viên gốc hiện có.
        var oldRootMember = new Member { Id = Guid.NewGuid(), FirstName = "Gốc Cũ", LastName = "Họ Gốc Cũ", FamilyId = familyId, IsRoot = true };
        _context.Members.Add(oldRootMember);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo một thành viên khác để cập nhật thành gốc mới.
        var memberToBecomeNewRoot = new Member { Id = Guid.NewGuid(), FirstName = "Gốc Mới", LastName = "Họ Gốc Mới", FamilyId = familyId, IsRoot = false };
        _context.Members.Add(memberToBecomeNewRoot);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateMemberCommand
        {
            Id = memberToBecomeNewRoot.Id,
            FirstName = "Gốc Mới",
            LastName = "Họ Gốc Mới",
            FamilyId = familyId,
            IsRoot = true
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();

        var newRootMember = await _context.Members.FindAsync(memberToBecomeNewRoot.Id);
        newRootMember.Should().NotBeNull();
        newRootMember!.IsRoot.Should().BeTrue();

        var updatedOldRootMember = await _context.Members.FindAsync(oldRootMember.Id);
        updatedOldRootMember.Should().NotBeNull();
        updatedOldRootMember!.IsRoot.Should().BeFalse();

        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(familyId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
