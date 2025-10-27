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



    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi không tìm thấy thành viên cần xóa.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Đảm bảo _context.Members không chứa thành viên cần xóa.
    ///               Tạo một DeleteMemberCommand với Id của một thành viên không tồn tại.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống không thể xóa
    /// một thành viên không tồn tại, ngăn chặn các lỗi tham chiếu và đảm bảo tính toàn vẹn dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        var command = _fixture.Create<DeleteMemberCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(string.Format(backend.Application.Common.Constants.ErrorMessages.NotFound, $"Member with ID {command.Id}"));
        result.ErrorSource.Should().Be(backend.Application.Common.Constants.ErrorSources.NotFound);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi người dùng không có quyền quản lý gia đình mà thành viên thuộc về.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một thành viên. Thiết lập _mockAuthorizationService để IsAdmin trả về false
    ///               và CanManageFamily trả về false cho FamilyId của thành viên.
    ///    - Act: Gọi phương thức Handle với DeleteMemberCommand của thành viên đó.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng chỉ những người dùng
    /// có quyền quản lý gia đình mới có thể xóa thành viên, bảo vệ dữ liệu gia đình khỏi truy cập trái phép.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserCannotManageFamily()
    {
        var member = _fixture.Create<Member>();
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(member.FamilyId)).Returns(false);

        var command = new DeleteMemberCommand(member.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(backend.Application.Common.Constants.ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(backend.Application.Common.Constants.ErrorSources.Forbidden);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler xóa thành viên thành công
    /// khi người dùng hiện tại là quản trị viên (Admin).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một thành viên và thêm vào context. Thiết lập _mockAuthorizationService để IsAdmin trả về true.
    ///               Thiết lập _mockFamilyTreeService để UpdateFamilyStats trả về Task.CompletedTask.
    ///    - Act: Gọi phương thức Handle với DeleteMemberCommand của thành viên đó.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công.
    ///              Kiểm tra rằng thành viên đã bị xóa khỏi context.
    ///              Xác minh rằng _mockFamilyTreeService.UpdateFamilyStats đã được gọi một lần.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng người dùng có vai trò quản trị viên
    /// có thể xóa thành viên một cách thành công và các thay đổi được lưu trữ chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldDeleteMemberSuccessfully_WhenAdminUser()
    {
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var member = new Member { Id = memberId, FamilyId = familyId, FirstName = "Test", LastName = "Member", Code = "M001" };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _context.Members.Count().Should().Be(1);

        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(true);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(It.IsAny<Guid>())).Returns(true);
        _mockFamilyTreeService.Setup(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .Returns(Task.CompletedTask);

        var command = new DeleteMemberCommand(memberId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _context.Members.Should().NotContain(m => m.Id == member.Id);

        var memberAfterDeletionAttempt = await _context.Members.FirstOrDefaultAsync(m => m.Id == member.Id);
        memberAfterDeletionAttempt.Should().BeNull();
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(familyId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler xóa thành viên thành công
    /// khi người dùng hiện tại có quyền quản lý gia đình (Manager).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một thành viên và thêm vào context. Thiết lập _mockAuthorizationService để IsAdmin trả về false
    ///               và CanManageFamily trả về true cho FamilyId của thành viên. Thiết lập _mockFamilyTreeService
    ///               để UpdateFamilyStats trả về Task.CompletedTask.
    ///    - Act: Gọi phương thức Handle với DeleteMemberCommand của thành viên đó.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công.
    ///              Kiểm tra rằng thành viên đã bị xóa khỏi context.
    ///              Xác minh rằng _mockFamilyTreeService.UpdateFamilyStats đã được gọi một lần.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng người dùng có vai trò quản lý gia đình
    /// có thể xóa thành viên một cách thành công và các thay đổi được lưu trữ chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldDeleteMemberSuccessfully_WhenManagerUser()
    {
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var member = new Member { Id = memberId, FamilyId = familyId, FirstName = "Test", LastName = "Member", Code = "M001" };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(familyId)).Returns(true);
        _mockFamilyTreeService.Setup(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .Returns(Task.CompletedTask);

        var command = new DeleteMemberCommand(memberId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _context.Members.Should().NotContain(m => m.Id == member.Id);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(familyId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
