using AutoFixture;
using backend.Application.NotificationTemplates.Commands.DeleteNotificationTemplate;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.NotificationTemplates.Commands.DeleteNotificationTemplate;

public class DeleteNotificationTemplateCommandValidatorTests
{
    private readonly Fixture _fixture;

    public DeleteNotificationTemplateCommandValidatorTests()
    {
        _fixture = new Fixture();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi trường Id của DeleteNotificationTemplateCommand bị để trống.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một DeleteNotificationTemplateCommand với Id là Guid.Empty.
    ///               Khởi tạo DeleteNotificationTemplateCommandValidator.
    ///    - Act: Gọi phương thức Validate của validator với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "ID không được để trống.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Trường Id là bắt buộc và không được để trống.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_EmptyId_ShouldReturnValidationError()
    {
        // Arrange
        var command = new DeleteNotificationTemplateCommand(Guid.Empty);
        var validator = new DeleteNotificationTemplateCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage == "ID không được để trống.");
    }
}
