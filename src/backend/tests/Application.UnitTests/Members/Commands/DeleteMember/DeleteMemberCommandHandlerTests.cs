using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.DeleteMember;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandlerTests : TestBase
{
    private readonly Mock<IFamilyTreeService> _mockFamilyTreeService;
    private readonly DeleteMemberCommandHandler _handler;

    public DeleteMemberCommandHandlerTests()
    {
        _mockFamilyTreeService = new Mock<IFamilyTreeService>();

        _handler = new DeleteMemberCommandHandler(
            _context,
            _mockAuthorizationService.Object,
            _mockFamilyTreeService.Object
        );
    }



    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi thành viên không tồn tại.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Đảm bảo _context.Members không chứa thành viên cần xóa.
        // 2. Act: Gọi phương thức Handle với một DeleteMemberCommand có Id không tồn tại.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var command = _fixture.Create<DeleteMemberCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Member with ID {command.Id} not found.");
        // 💡 Giải thích: Handler phải kiểm tra sự tồn tại của thành viên trước khi xóa.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserCannotManageFamily()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi người dùng không có quyền quản lý gia đình.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một thành viên, mock GetCurrentUserProfileAsync trả về profile hợp lệ, IsAdmin trả về false, CanManageFamily trả về false.
        // 2. Act: Gọi phương thức Handle với DeleteMemberCommand của thành viên đó.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var member = _fixture.Create<Member>();
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(member.FamilyId)).Returns(false);

        var command = new DeleteMemberCommand(member.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied. Only family managers can delete members.");
        // 💡 Giải thích: Người dùng phải có quyền quản lý gia đình để xóa thành viên.
    }

    [Fact]
    public async Task Handle_ShouldDeleteMemberSuccessfully_WhenAdminUser()
    {
        // 🎯 Mục tiêu của test: Xác minh handler xóa thành viên thành công khi người dùng là admin.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một thành viên, mock GetCurrentUserProfileAsync trả về profile hợp lệ, IsAdmin trả về true.
        // 2. Act: Gọi phương thức Handle với DeleteMemberCommand của thành viên đó.
        // 3. Assert: Kiểm tra kết quả trả về là thành công, thành viên bị xóa khỏi context, và các service khác được gọi.
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var member = new Member { Id = memberId, FamilyId = familyId, FirstName = "Test", LastName = "Member", Code = "M001" };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _context.Members.Count().Should().Be(1);

        var userProfile = new UserProfile { Id = Guid.NewGuid() };
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(true);

        var command = new DeleteMemberCommand(memberId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _context.Members.Should().NotContain(m => m.Id == member.Id);

        // Thêm assertion này để kiểm tra xem memberToDelete có bị null không
        var memberAfterDeletionAttempt = await _context.Members.FirstOrDefaultAsync(m => m.Id == member.Id);
        memberAfterDeletionAttempt.Should().BeNull(); // Mong đợi là null nếu xóa thành công
        // 💡 Giải thích: Người dùng admin có quyền xóa thành viên mà không cần kiểm tra quyền quản lý gia đình cụ thể.
    }

    [Fact]
    public async Task Handle_ShouldDeleteMemberSuccessfully_WhenManagerUser()
    {
        // 🎯 Mục tiêu của test: Xác minh handler xóa thành viên thành công khi người dùng có quyền quản lý gia đình.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một thành viên, mock GetCurrentUserProfileAsync trả về profile hợp lệ, IsAdmin trả về false, CanManageFamily trả về true.
        // 2. Act: Gọi phương thức Handle với DeleteMemberCommand của thành viên đó.
        // 3. Assert: Kiểm tra kết quả trả về là thành công, thành viên bị xóa khỏi context, và các service khác được gọi.
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var member = new Member { Id = memberId, FamilyId = familyId, FirstName = "Test", LastName = "Member", Code = "M001" };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(familyId)).Returns(true);

        var command = new DeleteMemberCommand(memberId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _context.Members.Should().NotContain(m => m.Id == member.Id);
        // 💡 Giải thích: Người dùng có quyền quản lý gia đình có thể xóa thành viên.
    }
}
