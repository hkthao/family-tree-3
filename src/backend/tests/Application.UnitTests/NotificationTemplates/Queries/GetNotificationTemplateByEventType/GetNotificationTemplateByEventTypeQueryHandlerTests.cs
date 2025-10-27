using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.NotificationTemplates.Queries;
using backend.Application.NotificationTemplates.Queries.GetNotificationTemplateByEventType;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.NotificationTemplates.Queries.GetNotificationTemplateByEventType;

public class GetNotificationTemplateByEventTypeQueryHandlerTests : TestBase
{
    private readonly GetNotificationTemplateByEventTypeQueryHandler _handler;

    public GetNotificationTemplateByEventTypeQueryHandlerTests()
    {
        _handler = new GetNotificationTemplateByEventTypeQueryHandler(_context, _mapper);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi không tìm thấy NotificationTemplate khớp với EventType và Channel được cung cấp.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Đảm bảo _context.NotificationTemplates không chứa template khớp.
    ///               Tạo một GetNotificationTemplateByEventTypeQuery với EventType và Channel bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp không tìm thấy template, ngăn chặn lỗi và cung cấp thông báo lỗi rõ ràng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotificationTemplateNotFound()
    {
        // Arrange
        var query = _fixture.Create<GetNotificationTemplateByEventTypeQuery>();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Notification template not found.");
        result.ErrorSource.Should().Be("NotFound");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về NotificationTemplateDto thành công
    /// khi tìm thấy một template khớp với EventType và Channel được cung cấp và đang hoạt động.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một NotificationTemplate đang hoạt động và thêm vào _context.
    ///               Tạo một GetNotificationTemplateByEventTypeQuery với EventType và Channel khớp.
    ///    - Act: Gọi phương thức Handle của handler với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và chứa NotificationTemplateDto được ánh xạ chính xác.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống có thể
    /// truy xuất và ánh xạ một template đang hoạt động dựa trên EventType và Channel một cách chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnNotificationTemplate_WhenFound()
    {
        // Arrange
        var eventType = NotificationType.FamilyCreated;
        var channel = NotificationChannel.Email;
        var notificationTemplate = _fixture.Build<NotificationTemplate>()
            .With(nt => nt.EventType, eventType)
            .With(nt => nt.Channel, channel)
            .With(nt => nt.IsActive, true)
            .Create();
        _context.NotificationTemplates.Add(notificationTemplate);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetNotificationTemplateByEventTypeQuery
        {
            EventType = eventType,
            Channel = channel
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.EventType.Should().Be(eventType);
        result.Value.Channel.Should().Be(channel);
        result.Value.Id.Should().Be(notificationTemplate.Id);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi tìm thấy một template khớp với EventType và Channel nhưng không hoạt động (IsActive = false).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một NotificationTemplate không hoạt động và thêm vào _context.
    ///               Tạo một GetNotificationTemplateByEventTypeQuery với EventType và Channel khớp.
    ///    - Act: Gọi phương thức Handle của handler với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống chỉ trả về
    /// các template đang hoạt động, bỏ qua các template không hoạt động.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotificationTemplateIsInactive()
    {
        // Arrange
        var eventType = NotificationType.FamilyCreated;
        var channel = NotificationChannel.Email;
        var notificationTemplate = _fixture.Build<NotificationTemplate>()
            .With(nt => nt.EventType, eventType)
            .With(nt => nt.Channel, channel)
            .With(nt => nt.IsActive, false)
            .Create();
        _context.NotificationTemplates.Add(notificationTemplate);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetNotificationTemplateByEventTypeQuery
        {
            EventType = eventType,
            Channel = channel
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Notification template not found.");
        result.ErrorSource.Should().Be("NotFound");
    }
}
