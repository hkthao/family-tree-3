using backend.Application.Common.Constants;
using AutoFixture;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Events;
using backend.Domain.Events.Families;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandHandlerTests : TestBase
{
    private readonly DeleteFamilyCommandHandler _handler;

    public DeleteFamilyCommandHandlerTests()
    {
        _handler = new DeleteFamilyCommandHandler(_context, _mockAuthorizationService.Object);
    }


    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi người dùng không phải là quản trị viên và không có quyền quản lý gia đình.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile giả lập. Thiết lập _mockAuthorizationService.IsAdmin để trả về false
    ///               và _mockAuthorizationService.CanManageFamily để trả về false.
    ///    - Act: Gọi phương thức Handle của handler với một DeleteFamilyCommand bất kỳ.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại, với thông báo lỗi là ErrorMessages.AccessDenied
    ///              và ErrorSource là ErrorSources.Forbidden.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Chỉ những người dùng có quyền (quản trị viên hoặc người quản lý gia đình)
    /// mới có thể xóa gia đình.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotHavePermission()
    {
        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        _mockUser.Setup(x => x.Id).Returns(userProfile.Id);
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(It.IsAny<Guid>()))
                                 .Returns(false);

        var command = _fixture.Create<DeleteFamilyCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi gia đình cần xóa không tìm thấy trong cơ sở dữ liệu.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile giả lập. Thiết lập _mockAuthorizationService.IsAdmin để trả về true.
    ///               Đảm bảo không có Family nào trong DB khớp với ID của command.
    ///    - Act: Gọi phương thức Handle của handler với một DeleteFamilyCommand bất kỳ.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại, với thông báo lỗi là ErrorMessages.AccessDenied
    ///              và ErrorSource là ErrorSources.Forbidden.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Hệ thống không thể xóa một gia đình không tồn tại,
    /// ngăn chặn các lỗi tham chiếu và đảm bảo tính toàn vẹn dữ liệu. Trong trường hợp này, lỗi truy cập bị từ chối
    /// được trả về vì người dùng không có quyền truy cập vào một gia đình không tồn tại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyNotFound()
    {
        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        _mockUser.Setup(x => x.Id).Returns(userProfile.Id);
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(true); // Assume admin for simplicity in this test

        // Ensure no Family exists for this ID
        _context.Families.RemoveRange(_context.Families);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = _fixture.Create<DeleteFamilyCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler xóa thành công một gia đình
    /// khi yêu cầu hợp lệ và người dùng là quản trị viên.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile giả lập và một Family hiện có, sau đó thêm vào DB.
    ///               Thiết lập _mockAuthorizationService để trả về IsAdmin là true.
    ///    - Act: Gọi phương thức Handle của handler với một DeleteFamilyCommand với ID của gia đình hiện có.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem gia đình đã bị xóa khỏi DB.
    ///              Kiểm tra xem FamilyDeletedEvent và FamilyStatsUpdatedEvent đã được kích hoạt.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Một quản trị viên có thể xóa thành công một gia đình hiện có
    /// và các hoạt động liên quan được ghi lại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldDeleteFamilySuccessfully_WhenValidRequestAndUserIsAdmin()
    {
        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        var existingFamily = _fixture.Create<Family>();
        _context.Families.Add(existingFamily);
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync(CancellationToken.None);
        _mockUser.Setup(x => x.Id).Returns(userProfile.Id);

        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(true);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(existingFamily.Id)).Returns(true);

        var command = new DeleteFamilyCommand(existingFamily.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var deletedFamily = await _context.Families.FirstOrDefaultAsync(e => e.Id == existingFamily.Id);
        deletedFamily.Should().BeNull();

        existingFamily.DomainEvents.Should().ContainSingle(e => e is FamilyDeletedEvent);
        existingFamily.DomainEvents.Should().ContainSingle(e => e is FamilyStatsUpdatedEvent);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler xóa thành công một gia đình
    /// khi yêu cầu hợp lệ và người dùng có quyền quản lý gia đình (nhưng không phải là quản trị viên).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile giả lập và một Family hiện có, sau đó thêm vào DB.
    ///               Thiết lập _mockAuthorizationService để trả về IsAdmin là false và CanManageFamily là true.
    ///    - Act: Gọi phương thức Handle của handler với một DeleteFamilyCommand với ID của gia đình hiện có.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem gia đình đã bị xóa khỏi DB.
    ///              Kiểm tra xem FamilyDeletedEvent và FamilyStatsUpdatedEvent đã được kích hoạt.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Một người dùng có quyền quản lý gia đình có thể xóa thành công
    /// một gia đình hiện có và các hoạt động liên quan được ghi lại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldDeleteFamilySuccessfully_WhenValidRequestAndUserCanManageFamily()
    {
        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        var existingFamily = _fixture.Create<Family>();
        _context.Families.Add(existingFamily);
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockUser.Setup(x => x.Id).Returns(userProfile.Id);
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(existingFamily.Id))
                                 .Returns(true);

        var command = new DeleteFamilyCommand(existingFamily.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var deletedFamily = await _context.Families.FindAsync(existingFamily.Id);
        deletedFamily.Should().BeNull();

        existingFamily.DomainEvents.Should().ContainSingle(e => e is FamilyDeletedEvent);
        existingFamily.DomainEvents.Should().ContainSingle(e => e is FamilyStatsUpdatedEvent);
    }
}
