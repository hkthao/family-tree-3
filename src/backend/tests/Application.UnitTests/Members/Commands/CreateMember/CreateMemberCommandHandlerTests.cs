using backend.Application.Members.Commands.CreateMember;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.CreateMember;

public class CreateMemberCommandHandlerTests : TestBase
{
    private readonly CreateMemberCommandHandler _handler;

    public CreateMemberCommandHandlerTests()
    {
        _handler = new CreateMemberCommandHandler(
            _context,
            _mockUser.Object,
            _mockAuthorizationService.Object,
            _mockMediator.Object,
            _mockFamilyTreeService.Object);
    }

    private async Task ClearDatabaseAndSetupUser(string userId, Guid userProfileId, Guid familyId, bool isAdmin, bool canManageFamily)
    {
        _context.Events.RemoveRange(_context.Events);
        _context.Members.RemoveRange(_context.Members);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockUser.Setup(x => x.Id).Returns(userId);

        var userProfile = new UserProfile { Id = userProfileId, ExternalId = userId, Email = "test@example.com", Name = "Test User" };
        _context.UserProfiles.Add(userProfile);
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TEST_FAMILY" });
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = familyId, UserProfileId = userProfileId, Role = FamilyRole.Manager });
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(isAdmin);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId, userProfile)).Returns(canManageFamily);
    }

    /// <summary>
    /// Kiểm tra xem một thành viên có được tạo thành công khi tất cả các điều kiện hợp lệ.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một lệnh CreateMemberCommand hợp lệ được cung cấp
    /// và người dùng được xác thực với quyền quản lý gia đình, một thành viên mới sẽ được tạo,
    /// và các hoạt động liên quan được ghi lại.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Create_Member_Successfully()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        var command = new CreateMemberCommand
        {
            FirstName = "Thành viên",
            LastName = "Mới",
            FamilyId = familyId,
            IsRoot = false,
            Code = Guid.NewGuid().ToString()
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        var memberId = result.Value;

        var createdMember = await _context.Members.FindAsync(memberId);
        createdMember.Should().NotBeNull();
        createdMember!.FirstName.Should().Be("Thành viên");
        createdMember.LastName.Should().Be("Mới");

        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(familyId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi người dùng hiện tại không được xác thực.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng nếu _user.Id là null hoặc rỗng, handler sẽ trả về kết quả thất bại
    /// với thông báo lỗi xác thực.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserIsNotAuthenticated()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Members.RemoveRange(_context.Members);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockUser.Setup(x => x.Id).Returns((string)null!);

        var command = new CreateMemberCommand
        {
            FirstName = "Thành viên",
            LastName = "Mới",
            FamilyId = Guid.NewGuid(),
            IsRoot = false
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User is not authenticated.");

        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi UserProfile của người dùng không tồn tại.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng nếu UserProfile không được tìm thấy cho người dùng hiện tại,
    /// handler sẽ trả về kết quả thất bại với thông báo lỗi "không tìm thấy".
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserProfileNotFound()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Members.RemoveRange(_context.Members);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        var userId = Guid.NewGuid().ToString();
        _mockUser.Setup(x => x.Id).Returns(userId);
        // Không thêm UserProfile vào context, giả lập không tìm thấy.

        var command = new CreateMemberCommand
        {
            FirstName = "Thành viên",
            LastName = "Mới",
            FamilyId = Guid.NewGuid(),
            IsRoot = false
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");

        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi người dùng không có quyền tạo thành viên trong gia đình.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng chỉ những người dùng có quyền quản lý gia đình mới có thể tạo thành viên.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserIsNotAuthorizedToManageFamily()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, false); // User is not authorized

        var command = new CreateMemberCommand
        {
            FirstName = "Thành viên",
            LastName = "Mới",
            FamilyId = familyId,
            IsRoot = false
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied. Only family managers can create members.");

        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Kiểm tra logic khi tạo một thành viên gốc mới (IsRoot = true).
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một thành viên mới được tạo với IsRoot = true,
    /// nếu đã có một thành viên gốc khác trong cùng gia đình, thành viên gốc cũ sẽ được đặt lại IsRoot = false.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_SetOldRootToNonRoot_WhenNewRootIsCreated()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        // Tạo một thành viên gốc hiện có.
        var oldRootMember = new Member { Id = Guid.NewGuid(), FirstName = "Gốc Cũ", LastName = "Họ Gốc Cũ", FamilyId = familyId, IsRoot = true, Code = Guid.NewGuid().ToString() };
        _context.Members.Add(oldRootMember);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new CreateMemberCommand
        {
            FirstName = "Gốc Mới",
            LastName = "",
            FamilyId = familyId,
            IsRoot = true
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        var newRootMemberId = result.Value;

        var newRootMember = await _context.Members.FindAsync(newRootMemberId);
        newRootMember.Should().NotBeNull();
        newRootMember!.IsRoot.Should().BeTrue();

        var updatedOldRootMember = await _context.Members.FindAsync(oldRootMember.Id);
        updatedOldRootMember.Should().NotBeNull();
        updatedOldRootMember!.IsRoot.Should().BeFalse();

        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(familyId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
