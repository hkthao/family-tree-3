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

        // Lấy thành viên từ dữ liệu mẫu để cập nhật.
        var memberToUpdate = await _context.Members.FindAsync(williamId);
        memberToUpdate.Should().NotBeNull(); // Đảm bảo thành viên tồn tại

        var command = new UpdateMemberCommand
        {
            Id = memberToUpdate!.Id,
            FirstName = "Tên Mới",
            LastName = "Họ Mới",
            Gender = "Female",
            FamilyId = royalFamilyId
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
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(royalFamilyId, It.IsAny<CancellationToken>()), Times.Once);
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
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData

        var currentUserProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };
        _context.UserProfiles.Add(currentUserProfile);
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = royalFamilyId, UserProfileId = currentUserProfile.Id, Role = FamilyRole.Manager });
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(royalFamilyId, currentUserProfile)).Returns(true);

        var invalidId = Guid.NewGuid();
        var command = new UpdateMemberCommand { Id = invalidId, FirstName = "Tên", LastName = "Họ", FamilyId = royalFamilyId };

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
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData
        var williamId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef"); // Prince William ID from SeedSampleData

        var currentUserProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };
        _context.UserProfiles.Add(currentUserProfile);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(royalFamilyId, currentUserProfile)).Returns(false); // User is not authorized

        // Lấy thành viên từ dữ liệu mẫu để cập nhật.
        var memberToUpdate = await _context.Members.FindAsync(williamId);
        memberToUpdate.Should().NotBeNull(); // Đảm bảo thành viên tồn tại

        var command = new UpdateMemberCommand
        {
            Id = memberToUpdate!.Id,
            FirstName = "Tên Mới",
            LastName = "Họ Mới",
            FamilyId = royalFamilyId
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
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData
        var williamId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef"); // Prince William ID from SeedSampleData
        var harryId = Guid.Parse("a1b2c3d4-e5f6-3456-7890-1234567890ab"); // Prince Harry ID from SeedSampleData

        var currentUserProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };
        _context.UserProfiles.Add(currentUserProfile);
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = royalFamilyId, UserProfileId = currentUserProfile.Id, Role = FamilyRole.Manager });
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(royalFamilyId, currentUserProfile)).Returns(true);

        // Lấy thành viên gốc hiện có từ dữ liệu mẫu và đặt IsRoot = true.
        var oldRootMember = await _context.Members.FindAsync(williamId);
        oldRootMember.Should().NotBeNull();
        oldRootMember!.IsRoot = true;
        await _context.SaveChangesAsync(CancellationToken.None);

        // Lấy thành viên khác từ dữ liệu mẫu để cập nhật thành gốc mới.
        var memberToBecomeNewRoot = await _context.Members.FindAsync(harryId);
        memberToBecomeNewRoot.Should().NotBeNull();

        var command = new UpdateMemberCommand
        {
            Id = memberToBecomeNewRoot!.Id,
            FirstName = "Gốc Mới",
            LastName = "Họ Gốc Mới",
            FamilyId = royalFamilyId,
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
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(royalFamilyId, It.IsAny<CancellationToken>()), Times.Once);
    }
}