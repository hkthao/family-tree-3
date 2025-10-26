using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.UpdateFamily;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Events;
using backend.Domain.Events.Families;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandlerTests : TestBase
{
    private readonly UpdateFamilyCommandHandler _handler;

    public UpdateFamilyCommandHandlerTests()
    {
        _handler = new UpdateFamilyCommandHandler(_context, _mockAuthorizationService.Object);
    }



    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotHavePermission()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về một kết quả thất bại
        // khi người dùng không phải là quản trị viên và không có quyền quản lý gia đình.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập _mockAuthorizationService.IsAdmin để trả về false.
        // 2. Thiết lập _mockAuthorizationService.CanManageFamily để trả về false.
        // 3. Tạo một UpdateFamilyCommand bất kỳ.

        // Arrange
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(It.IsAny<Guid>()))
                                 .Returns(false);

        var command = _fixture.Create<UpdateFamilyCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User does not have permission to update this family.");
        result.ErrorSource.Should().Be("Forbidden");

        // 💡 Giải thích:
        // Test này đảm bảo rằng chỉ những người dùng có quyền (quản trị viên hoặc người quản lý gia đình)
        // mới có thể cập nhật thông tin gia đình.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyNotFound()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về một kết quả thất bại
        // khi gia đình cần cập nhật không tìm thấy trong cơ sở dữ liệu.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập _mockAuthorizationService.IsAdmin để trả về true (hoặc CanManageFamily trả về true).
        // 2. Đảm bảo không có Family nào trong DB khớp với ID của command.
        // 3. Tạo một UpdateFamilyCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thất bại.
        // 2. Kiểm tra thông báo lỗi phù hợp.

        // Arrange
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(true); // Assume admin for simplicity in this test

        // Ensure no Family exists for this ID
        _context.Families.RemoveRange(_context.Families);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = _fixture.Create<UpdateFamilyCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Family with ID {command.Id} not found.");
        result.ErrorSource.Should().Be("NotFound");

        // 💡 Giải thích:
        // Test này đảm bảo rằng hệ thống không thể cập nhật một gia đình không tồn tại,
        // ngăn chặn các lỗi tham chiếu và đảm bảo tính toàn vẹn dữ liệu.
    }

    [Fact]
    public async Task Handle_ShouldUpdateFamilySuccessfully_WhenValidRequestAndUserIsAdmin()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler cập nhật thành công thông tin gia đình
        // khi yêu cầu hợp lệ và người dùng là quản trị viên.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một Family hiện có, sau đó thêm vào DB.
        // 2. Thiết lập _mockAuthorizationService để trả về IsAdmin là true.
        // 3. Tạo một UpdateFamilyCommand với các giá trị mới.
        // 4. Thiết lập _mockMediator và _mockFamilyTreeService.

        // Arrange
        var existingFamily = _fixture.Create<Family>();
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(true);

        var command = _fixture.Build<UpdateFamilyCommand>()
                               .With(c => c.Id, existingFamily.Id)
                               .With(c => c.Name, "Updated Family Name")
                               .With(c => c.Description, "Updated Description")
                               .With(c => c.Address, "Updated Address")
                               .With(c => c.Visibility, FamilyVisibility.Private.ToString())
                               .Create();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedFamily = await _context.Families.FindAsync(existingFamily.Id);
        updatedFamily.Should().NotBeNull();
        updatedFamily!.Name.Should().Be(command.Name);
        updatedFamily.Description.Should().Be(command.Description);
        updatedFamily.Address.Should().Be(command.Address);
        updatedFamily.Visibility.Should().Be(command.Visibility);

        updatedFamily.DomainEvents.Should().ContainSingle(e => e is FamilyUpdatedEvent);
        updatedFamily.DomainEvents.Should().ContainSingle(e => e is FamilyStatsUpdatedEvent);

        // 💡 Giải thích:
        // Test này xác minh rằng một quản trị viên có thể cập nhật thành công tất cả các thuộc tính
        // của một gia đình hiện có và các hoạt động liên quan được ghi lại.
    }

    [Fact]
    public async Task Handle_ShouldUpdateFamilySuccessfully_WhenValidRequestAndUserCanManageFamily()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler cập nhật thành công thông tin gia đình
        // khi yêu cầu hợp lệ và người dùng có quyền quản lý gia đình (nhưng không phải là quản trị viên).

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một Family hiện có, sau đó thêm vào DB.
        // 2. Thiết lập _mockAuthorizationService để trả về IsAdmin là false,
        //    và CanManageFamily là true.
        // 3. Tạo một UpdateFamilyCommand với các giá trị mới.
        // 4. Thiết lập _mockMediator và _mockFamilyTreeService.

        // Arrange
        var existingFamily = _fixture.Create<Family>();
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(existingFamily.Id))
                                 .Returns(true);

        var command = _fixture.Build<UpdateFamilyCommand>()
                               .With(c => c.Id, existingFamily.Id)
                               .With(c => c.Name, "Updated Family Name by Manager")
                               .With(c => c.Description, "Updated Description by Manager")
                               .With(c => c.Address, "Updated Address by Manager")
                               .With(c => c.Visibility, FamilyVisibility.Public.ToString())
                               .Create();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedFamily = await _context.Families.FindAsync(existingFamily.Id);
        updatedFamily.Should().NotBeNull();
        updatedFamily!.Name.Should().Be(command.Name);
        updatedFamily.Description.Should().Be(command.Description);
        updatedFamily.Address.Should().Be(command.Address);
        updatedFamily.Visibility.Should().Be(command.Visibility);

        updatedFamily.DomainEvents.Should().ContainSingle(e => e is FamilyUpdatedEvent);
        updatedFamily.DomainEvents.Should().ContainSingle(e => e is FamilyStatsUpdatedEvent);

        // 💡 Giải thích:
        // Test này xác minh rằng một người dùng có quyền quản lý gia đình có thể cập nhật thành công
        // một gia đình hiện có và các hoạt động liên quan được ghi lại.
    }
}
