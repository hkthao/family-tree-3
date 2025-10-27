using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.NotificationTemplates.Commands.UpdateNotificationTemplate;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.NotificationTemplates.Commands.UpdateNotificationTemplate;

public class UpdateNotificationTemplateCommandHandlerTests : TestBase
{
    private readonly UpdateNotificationTemplateCommandHandler _handler;

    public UpdateNotificationTemplateCommandHandlerTests()
    {
        _handler = new UpdateNotificationTemplateCommandHandler(_context);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi không tìm thấy NotificationTemplate cần cập nhật.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Đảm bảo _context.NotificationTemplates không chứa template cần cập nhật.
    ///               Tạo một UpdateNotificationTemplateCommand với Id của một template không tồn tại.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống không thể cập nhật
    /// một template không tồn tại, ngăn chặn các lỗi tham chiếu và đảm bảo tính toàn vẹn dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotificationTemplateNotFound()
    {
        // Arrange
        var command = _fixture.Create<UpdateNotificationTemplateCommand>(); // ID không tồn tại

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("NotificationTemplate not found.");
        result.ErrorSource.Should().Be("NotFound");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler cập nhật NotificationTemplate thành công
    /// khi template tồn tại trong cơ sở dữ liệu và command hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một NotificationTemplate và thêm vào _context.
    ///               Tạo một UpdateNotificationTemplateCommand với Id của template vừa tạo
    ///               và các giá trị mới cho các thuộc tính.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công.
    ///              Kiểm tra rằng NotificationTemplate đã được cập nhật trong _context.NotificationTemplates
    ///              với các giá trị mới.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Khi một command hợp lệ được cung cấp và template tồn tại,
    /// hệ thống phải cập nhật template đó trong cơ sở dữ liệu và thông báo thành công.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldUpdateNotificationTemplateSuccessfully()
    {
        // Arrange
        var notificationTemplate = _fixture.Create<NotificationTemplate>();
        _context.NotificationTemplates.Add(notificationTemplate);
        await _context.SaveChangesAsync(CancellationToken.None);

        var updatedEventType = NotificationType.MemberUpdated;
        var updatedChannel = NotificationChannel.SMS;
        var updatedSubject = "Updated Subject";
        var updatedBody = "Updated Body Content";
        var updatedFormat = TemplateFormat.PlainText;
        var updatedLanguageCode = "fr";
        var updatedIsActive = false;

        var command = new UpdateNotificationTemplateCommand
        {
            Id = notificationTemplate.Id,
            EventType = updatedEventType,
            Channel = updatedChannel,
            Subject = updatedSubject,
            Body = updatedBody,
            Format = updatedFormat,
            LanguageCode = updatedLanguageCode,
            IsActive = updatedIsActive
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedTemplate = await _context.NotificationTemplates.FirstOrDefaultAsync(nt => nt.Id == notificationTemplate.Id);
        updatedTemplate.Should().NotBeNull();
        updatedTemplate!.EventType.Should().Be(updatedEventType);
        updatedTemplate.Channel.Should().Be(updatedChannel);
        updatedTemplate.Subject.Should().Be(updatedSubject);
        updatedTemplate.Body.Should().Be(updatedBody);
        updatedTemplate.Format.Should().Be(updatedFormat);
        updatedTemplate.LanguageCode.Should().Be(updatedLanguageCode);
        updatedTemplate.IsActive.Should().Be(updatedIsActive);
    }
}
