using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Application.UnitTests.Common;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandHandlerTests : TestBase
{
    private readonly DeleteFamilyCommandHandler _handler;
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IFamilyTreeService> _mockFamilyTreeService;

    public DeleteFamilyCommandHandlerTests()
    {
        _mockAuthorizationService = _fixture.Freeze<Mock<IAuthorizationService>>();
        _mockMediator = _fixture.Freeze<Mock<IMediator>>();
        _mockFamilyTreeService = _fixture.Freeze<Mock<IFamilyTreeService>>();

        _handler = new DeleteFamilyCommandHandler(_context, _mockAuthorizationService.Object, _mockMediator.Object, _mockFamilyTreeService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserProfileNotFound()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về một kết quả thất bại
        // khi UserProfile của người dùng được xác thực không tìm thấy trong cơ sở dữ liệu.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập _mockAuthorizationService để trả về null cho GetCurrentUserProfileAsync.
        // 2. Tạo một DeleteFamilyCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thất bại.
        // 2. Kiểm tra thông báo lỗi phù hợp.

        // Arrange
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                                 .ReturnsAsync((UserProfile)null!); // UserProfile not found

        var command = _fixture.Create<DeleteFamilyCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User profile not found.");
        result.ErrorSource.Should().Be("NotFound");

        // 💡 Giải thích:
        // Test này đảm bảo rằng nếu hồ sơ người dùng không tồn tại trong hệ thống,
        // yêu cầu xóa gia đình sẽ thất bại để ngăn chặn việc thao tác dữ liệu không hợp lệ.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotHavePermission()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về một kết quả thất bại
        // khi người dùng không phải là quản trị viên và không có quyền quản lý gia đình.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile giả lập và thiết lập _mockAuthorizationService để trả về nó.
        // 2. Thiết lập _mockAuthorizationService.IsAdmin để trả về false.
        // 3. Thiết lập _mockAuthorizationService.CanManageFamily để trả về false.
        // 4. Tạo một DeleteFamilyCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thất bại.
        // 2. Kiểm tra thông báo lỗi phù hợp.

        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(userProfile);
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(It.IsAny<Guid>(), It.IsAny<UserProfile>()))
                                 .Returns(false);

        var command = _fixture.Create<DeleteFamilyCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User does not have permission to delete this family.");
        result.ErrorSource.Should().Be("Forbidden");

        // 💡 Giải thích:
        // Test này đảm bảo rằng chỉ những người dùng có quyền (quản trị viên hoặc người quản lý gia đình)
        // mới có thể xóa gia đình.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyNotFound()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về một kết quả thất bại
        // khi gia đình cần xóa không tìm thấy trong cơ sở dữ liệu.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile giả lập và thiết lập _mockAuthorizationService để trả về nó.
        // 2. Thiết lập _mockAuthorizationService.IsAdmin để trả về true (hoặc CanManageFamily trả về true).
        // 3. Đảm bảo không có Family nào trong DB khớp với ID của command.
        // 4. Tạo một DeleteFamilyCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thất bại.
        // 2. Kiểm tra thông báo lỗi phù hợp.

        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(userProfile);
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
        result.Error.Should().Contain($"Family with ID {command.Id} not found.");
        result.ErrorSource.Should().Be("NotFound");

        // 💡 Giải thích:
        // Test này đảm bảo rằng hệ thống không thể xóa một gia đình không tồn tại,
        // ngăn chặn các lỗi tham chiếu và đảm bảo tính toàn vẹn dữ liệu.
    }

    [Fact]
    public async Task Handle_ShouldDeleteFamilySuccessfully_WhenValidRequestAndUserIsAdmin()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler xóa thành công một gia đình
        // khi yêu cầu hợp lệ và người dùng là quản trị viên.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile giả lập và một Family hiện có, sau đó thêm vào DB.
        // 2. Thiết lập _mockAuthorizationService để trả về UserProfile và IsAdmin là true.
        // 3. Tạo một DeleteFamilyCommand với ID của gia đình hiện có.
        // 4. Thiết lập _mockMediator và _mockFamilyTreeService.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem gia đình đã bị xóa khỏi DB.
        // 3. Kiểm tra xem RecordActivityCommand và UpdateFamilyStats đã được gọi.

        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        var existingFamily = _fixture.Create<Family>();
        _context.Families.Add(existingFamily);
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockMediator.Setup(m => m.Send(It.IsAny<RecordActivityCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));
        _mockFamilyTreeService.Setup(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .Returns(Task.CompletedTask);
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(true);

        var command = new DeleteFamilyCommand(existingFamily.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var deletedFamily = await _context.Families.FirstOrDefaultAsync(e => e.Id == existingFamily.Id);
        deletedFamily.Should().BeNull();

        _mockMediator.Verify(m => m.Send(It.IsAny<RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(existingFamily.Id, It.IsAny<CancellationToken>()), Times.Once);

        // 💡 Giải thích:
        // Test này xác minh rằng một quản trị viên có thể xóa thành công một gia đình hiện có
        // và các hoạt động liên quan được ghi lại.
    }

    [Fact]
    public async Task Handle_ShouldDeleteFamilySuccessfully_WhenValidRequestAndUserCanManageFamily()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler xóa thành công một gia đình
        // khi yêu cầu hợp lệ và người dùng có quyền quản lý gia đình (nhưng không phải là quản trị viên).

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile giả lập và một Family hiện có, sau đó thêm vào DB.
        // 2. Thiết lập _mockAuthorizationService để trả về UserProfile, IsAdmin là false,
        //    và CanManageFamily là true.
        // 3. Tạo một DeleteFamilyCommand với ID của gia đình hiện có.
        // 4. Thiết lập _mockMediator và _mockFamilyTreeService.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem gia đình đã bị xóa khỏi DB.
        // 3. Kiểm tra xem RecordActivityCommand và UpdateFamilyStats đã được gọi.

        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        var existingFamily = _fixture.Create<Family>();
        _context.Families.Add(existingFamily);
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(userProfile);
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(existingFamily.Id, userProfile))
                                 .Returns(true);
        _mockMediator.Setup(m => m.Send(It.IsAny<RecordActivityCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));
        _mockFamilyTreeService.Setup(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .Returns(Task.CompletedTask);

        var command = new DeleteFamilyCommand(existingFamily.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var deletedFamily = await _context.Families.FindAsync(existingFamily.Id);
        deletedFamily.Should().BeNull();

        _mockMediator.Verify(m => m.Send(It.IsAny<RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(existingFamily.Id, It.IsAny<CancellationToken>()), Times.Once);

        // 💡 Giải thích:
        // Test này xác minh rằng một người dùng có quyền quản lý gia đình có thể xóa thành công
        // một gia đình hiện có và các hoạt động liên quan được ghi lại.
    }
}
