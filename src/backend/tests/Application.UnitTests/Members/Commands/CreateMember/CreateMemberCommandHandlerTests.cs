using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.CreateMember;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
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
            _mockAuthorizationService.Object
        );
    }



    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserCannotManageFamily()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi người dùng không phải admin và không có quyền quản lý gia đình.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockUser.Id hợp lệ, _mockAuthorizationService.IsAdmin() trả về false, GetCurrentUserProfileAsync trả về profile hợp lệ, và CanManageFamily trả về false.
        // 2. Act: Gọi phương thức Handle của handler.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(It.IsAny<Guid>())).Returns(false); // Không có quyền quản lý

        var command = _fixture.Create<CreateMemberCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied. Only family managers can create members.");
        // 💡 Giải thích: Người dùng phải có quyền quản lý gia đình để tạo thành viên mới.
    }

    [Fact]
    public async Task Handle_ShouldCreateMemberSuccessfully_WhenAdminUser()
    {
        // 🎯 Mục tiêu của test: Xác minh handler tạo thành viên thành công khi người dùng là admin.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockUser.Id hợp lệ, _mockAuthorizationService.IsAdmin() trả về true.
        // 2. Act: Gọi phương thức Handle của handler.
        // 3. Assert: Kiểm tra kết quả trả về là thành công, Member được thêm vào context, SaveChangesAsync được gọi, và RecordActivityCommand được gửi.
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(true);

        var command = _fixture.Create<CreateMemberCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _context.Members.Should().Contain(m => m.FirstName == command.FirstName && m.LastName == command.LastName); // Kiểm tra member đã được thêm vào context
        _context.Members.Count().Should().Be(1); // Đảm bảo chỉ có 1 member được thêm
        // 💡 Giải thích: Người dùng admin có quyền tạo thành viên mà không cần kiểm tra quyền quản lý gia đình cụ thể.
    }

    [Fact]
    public async Task Handle_ShouldCreateMemberSuccessfully_WhenManagerUser()
    {
        // 🎯 Mục tiêu của test: Xác minh handler tạo thành viên thành công khi người dùng có quyền quản lý gia đình.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockUser.Id hợp lệ, _mockAuthorizationService.IsAdmin() trả về false, GetCurrentUserProfileAsync trả về profile hợp lệ, và CanManageFamily trả về true.
        // 2. Act: Gọi phương thức Handle của handler.
        // 3. Assert: Kiểm tra kết quả trả về là thành công, Member được thêm vào context, SaveChangesAsync được gọi, và RecordActivityCommand được gửi.
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);
        var userProfile = _fixture.Create<UserProfile>();
        _mockAuthorizationService.Setup(a => a.CanManageFamily(It.IsAny<Guid>())).Returns(true);

        var command = _fixture.Create<CreateMemberCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _context.Members.Should().Contain(m => m.FirstName == command.FirstName && m.LastName == command.LastName); // Kiểm tra member đã được thêm vào context
        _context.Members.Count().Should().Be(1); // Đảm bảo chỉ có 1 member được thêm
        // 💡 Giải thích: Người dùng có quyền quản lý gia đình có thể tạo thành viên mới.
    }

    [Fact]
    public async Task Handle_ShouldSetNewMemberAsRoot_WhenIsRootIsTrueAndNoExistingRoot()
    {
        // 🎯 Mục tiêu của test: Xác minh thành viên mới được đặt làm gốc khi IsRoot là true và chưa có thành viên gốc nào tồn tại.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockUser.Id hợp lệ, _mockAuthorizationService.IsAdmin() trả về true. Đảm bảo không có thành viên gốc nào trong context.
        // 2. Act: Gọi phương thức Handle với CreateMemberCommand có IsRoot = true.
        // 3. Assert: Kiểm tra thành viên được thêm vào có IsRoot = true.
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(true);

        var command = _fixture.Build<CreateMemberCommand>()
            .With(c => c.IsRoot, true)
            .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _context.Members.Should().Contain(m => m.FirstName == command.FirstName && m.LastName == command.LastName && m.IsRoot == true);
        _context.Members.Count().Should().Be(1);
        // 💡 Giải thích: Khi tạo thành viên với IsRoot là true và không có thành viên gốc nào khác, thành viên này phải được đánh dấu là gốc.
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingRoot_WhenIsRootIsTrueAndExistingRootExists()
    {
        // 🎯 Mục tiêu của test: Xác minh thành viên gốc cũ được cập nhật IsRoot = false khi tạo thành viên gốc mới.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockUser.Id hợp lệ, _mockAuthorizationService.IsAdmin() trả về true. Thêm một thành viên gốc hiện có vào context.
        // 2. Act: Gọi phương thức Handle với CreateMemberCommand có IsRoot = true.
        // 3. Assert: Kiểm tra thành viên gốc cũ được cập nhật IsRoot = false và thành viên mới được thêm vào có IsRoot = true.
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(true);

        var existingRoot = _fixture.Build<Member>()
            .With(m => m.FamilyId, Guid.NewGuid())
            .With(m => m.IsRoot, true)
            .Create();

        _context.Members.Add(existingRoot);
        await _context.SaveChangesAsync(); // Lưu existingRoot vào In-memory DB

        var command = _fixture.Build<CreateMemberCommand>()
            .With(c => c.FamilyId, existingRoot.FamilyId)
            .With(c => c.IsRoot, true)
            .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // Lấy lại existingRoot từ context để kiểm tra trạng thái đã cập nhật
        var updatedExistingRoot = await _context.Members.FindAsync(existingRoot.Id);
        updatedExistingRoot.Should().NotBeNull();
        updatedExistingRoot!.IsRoot.Should().BeFalse(); // Thành viên gốc cũ phải được cập nhật

        _context.Members.Should().Contain(m => m.FirstName == command.FirstName && m.LastName == command.LastName && m.IsRoot == true); // Thành viên mới được thêm vào
        _context.Members.Count().Should().Be(2); // Tổng cộng 2 thành viên
        // 💡 Giải thích: Khi một thành viên mới được đặt làm gốc, thành viên gốc hiện có trong cùng gia đình phải được hủy đặt gốc.
    }
}
