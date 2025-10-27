using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.NotificationTemplates.Commands.DeleteNotificationTemplate;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.NotificationTemplates.Commands.DeleteNotificationTemplate;

public class DeleteNotificationTemplateCommandHandlerTests : TestBase
{
    private readonly DeleteNotificationTemplateCommandHandler _handler;

    public DeleteNotificationTemplateCommandHandlerTests()
    {
        _handler = new DeleteNotificationTemplateCommandHandler(_context);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi không tìm thấy NotificationTemplate cần xóa.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Đảm bảo _context.NotificationTemplates không chứa template cần xóa.
    ///               Tạo một DeleteNotificationTemplateCommand với Id của một template không tồn tại.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống không thể xóa
    /// một template không tồn tại, ngăn chặn các lỗi tham chiếu và đảm bảo tính toàn vẹn dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotificationTemplateNotFound()
    {
        // Arrange
        var command = new DeleteNotificationTemplateCommand(Guid.NewGuid()); // ID không tồn tại

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("NotificationTemplate not found.");
        result.ErrorSource.Should().Be("NotFound");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler xóa NotificationTemplate thành công
    /// khi template tồn tại trong cơ sở dữ liệu.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một NotificationTemplate và thêm vào _context.
    ///               Tạo một DeleteNotificationTemplateCommand với Id của template vừa tạo.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công.
    ///              Kiểm tra rằng NotificationTemplate đã bị xóa khỏi _context.NotificationTemplates.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Khi một command hợp lệ được cung cấp và template tồn tại,
    /// hệ thống phải xóa template đó khỏi cơ sở dữ liệu và thông báo thành công.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldDeleteNotificationTemplateSuccessfully()
    {
        // Arrange
        var notificationTemplate = _fixture.Create<NotificationTemplate>();
        _context.NotificationTemplates.Add(notificationTemplate);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteNotificationTemplateCommand(notificationTemplate.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _context.NotificationTemplates.Should().NotContain(nt => nt.Id == notificationTemplate.Id);
    }
}
