using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.NotificationTemplates.Queries;
using backend.Application.NotificationTemplates.Queries.GetNotificationTemplateById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.NotificationTemplates.Queries.GetNotificationTemplateById;

public class GetNotificationTemplateByIdQueryHandlerTests : TestBase
{
    private readonly GetNotificationTemplateByIdQueryHandler _handler;

    public GetNotificationTemplateByIdQueryHandlerTests()
    {
        _handler = new GetNotificationTemplateByIdQueryHandler(_context, _mapper);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi không tìm thấy NotificationTemplate với Id được cung cấp.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Đảm bảo _context.NotificationTemplates không chứa template với Id được cung cấp.
    ///               Tạo một GetNotificationTemplateByIdQuery với một Id không tồn tại.
    ///    - Act: Gọi phương thức Handle của handler với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp không tìm thấy template, ngăn chặn lỗi và cung cấp thông báo lỗi rõ ràng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotificationTemplateNotFound()
    {
        // Arrange
        var query = new GetNotificationTemplateByIdQuery(Guid.NewGuid()); // ID không tồn tại

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("NotificationTemplate not found.");
        result.ErrorSource.Should().Be("NotFound");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về NotificationTemplateDto thành công
    /// khi tìm thấy một template với Id được cung cấp.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một NotificationTemplate và thêm vào _context.
    ///               Tạo một GetNotificationTemplateByIdQuery với Id của template vừa tạo.
    ///    - Act: Gọi phương thức Handle của handler với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và chứa NotificationTemplateDto được ánh xạ chính xác.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống có thể
    /// truy xuất và ánh xạ một template dựa trên Id một cách chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnNotificationTemplate_WhenFound()
    {
        // Arrange
        var notificationTemplate = _fixture.Create<NotificationTemplate>();
        _context.NotificationTemplates.Add(notificationTemplate);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetNotificationTemplateByIdQuery(notificationTemplate.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(notificationTemplate.Id);
        result.Value.EventType.Should().Be(notificationTemplate.EventType);
        result.Value.Channel.Should().Be(notificationTemplate.Channel);
        result.Value.Subject.Should().Be(notificationTemplate.Subject);
        result.Value.Body.Should().Be(notificationTemplate.Body);
        result.Value.Format.Should().Be(notificationTemplate.Format);
        result.Value.LanguageCode.Should().Be(notificationTemplate.LanguageCode);
        result.Value.IsActive.Should().Be(notificationTemplate.IsActive);
    }
}
