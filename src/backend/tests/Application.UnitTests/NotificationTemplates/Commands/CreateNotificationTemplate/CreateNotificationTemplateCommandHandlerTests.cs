using AutoFixture;
using backend.Application.NotificationTemplates.Commands.CreateNotificationTemplate;
using backend.Application.UnitTests.Common;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.NotificationTemplates.Commands.CreateNotificationTemplate;

public class CreateNotificationTemplateCommandHandlerTests : TestBase
{
    private readonly CreateNotificationTemplateCommandHandler _handler;

    public CreateNotificationTemplateCommandHandlerTests()
    {
        _handler = new CreateNotificationTemplateCommandHandler(_context);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng một lệnh CreateNotificationTemplateCommand hợp lệ
    /// sẽ tạo và lưu một NotificationTemplate mới vào cơ sở dữ liệu,
    /// và trả về Result.Success với Id của template đã tạo.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateNotificationTemplateCommand hợp lệ.
    ///               Sử dụng _context (in-memory database) từ TestBase.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và chứa Id của entity.
    ///              Kiểm tra rằng NotificationTemplate đã được thêm vào _context.NotificationTemplates.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Khi một command hợp lệ được cung cấp,
    /// hệ thống phải tạo một bản ghi NotificationTemplate mới và lưu nó vào cơ sở dữ liệu,
    /// sau đó thông báo thành công cùng với Id của bản ghi mới.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateAndReturnNotificationTemplateId()
    {
        // Arrange
        var command = _fixture.Build<CreateNotificationTemplateCommand>()
            .With(c => c.EventType, NotificationType.FamilyCreated)
            .With(c => c.Channel, NotificationChannel.Email)
            .With(c => c.Format, TemplateFormat.Html)
            .With(c => c.Subject, "Test Subject")
            .With(c => c.Body, "Test Body")
            .With(c => c.LanguageCode, "en")
            .With(c => c.IsActive, true)
            .Create();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _context.NotificationTemplates.Should().Contain(nt =>
            nt.Id == result.Value &&
            nt.EventType == command.EventType &&
            nt.Channel == command.Channel &&
            nt.Subject == command.Subject &&
            nt.Body == command.Body &&
            nt.Format == command.Format &&
            nt.LanguageCode == command.LanguageCode &&
            nt.IsActive == command.IsActive
        );
    }
}
