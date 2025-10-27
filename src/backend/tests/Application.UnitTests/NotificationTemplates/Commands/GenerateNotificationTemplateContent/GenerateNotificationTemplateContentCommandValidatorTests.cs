using AutoFixture;
using backend.Application.NotificationTemplates.Commands.GenerateNotificationTemplateContent;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.NotificationTemplates.Commands.GenerateNotificationTemplateContent;

public class GenerateNotificationTemplateContentCommandValidatorTests
{
    private readonly Fixture _fixture;

    public GenerateNotificationTemplateContentCommandValidatorTests()
    {
        _fixture = new Fixture();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi trường Prompt của GenerateNotificationTemplateContentCommand bị để trống.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GenerateNotificationTemplateContentCommand với Prompt là chuỗi rỗng.
    ///               Khởi tạo GenerateNotificationTemplateContentCommandValidator.
    ///    - Act: Gọi phương thức Validate của validator với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "Lời nhắc không được để trống.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Trường Prompt là bắt buộc và không được để trống.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_EmptyPrompt_ShouldReturnValidationError()
    {
        // Arrange
        var command = new GenerateNotificationTemplateContentCommand { Prompt = string.Empty };
        var validator = new GenerateNotificationTemplateContentCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Prompt" && e.ErrorMessage == "Lời nhắc không được để trống.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi trường Prompt của GenerateNotificationTemplateContentCommand quá ngắn.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GenerateNotificationTemplateContentCommand với Prompt có độ dài nhỏ hơn 10 ký tự.
    ///               Khởi tạo GenerateNotificationTemplateContentCommandValidator.
    ///    - Act: Gọi phương thức Validate của validator với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "Lời nhắc phải có ít nhất 10 ký tự.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Trường Prompt có độ dài tối thiểu là 10 ký tự.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_PromptTooShort_ShouldReturnValidationError()
    {
        // Arrange
        var command = new GenerateNotificationTemplateContentCommand { Prompt = "short" }; // Less than 10 characters
        var validator = new GenerateNotificationTemplateContentCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Prompt" && e.ErrorMessage == "Lời nhắc phải có ít nhất 10 ký tự.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi trường Prompt của GenerateNotificationTemplateContentCommand quá dài.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GenerateNotificationTemplateContentCommand với Prompt có độ dài lớn hơn 1000 ký tự.
    ///               Khởi tạo GenerateNotificationTemplateContentCommandValidator.
    ///    - Act: Gọi phương thức Validate của validator với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "Lời nhắc không được vượt quá 1000 ký tự.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Trường Prompt có độ dài tối đa là 1000 ký tự.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_PromptTooLong_ShouldReturnValidationError()
    {
        // Arrange
        var longPrompt = new string('a', 1001); // More than 1000 characters
        var command = new GenerateNotificationTemplateContentCommand { Prompt = longPrompt };
        var validator = new GenerateNotificationTemplateContentCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Prompt" && e.ErrorMessage == "Lời nhắc không được vượt quá 1000 ký tự.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator không trả về lỗi khi trường Prompt của GenerateNotificationTemplateContentCommand hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GenerateNotificationTemplateContentCommand với Prompt hợp lệ (độ dài từ 10 đến 1000 ký tự).
    ///               Khởi tạo GenerateNotificationTemplateContentCommandValidator.
    ///    - Act: Gọi phương thức Validate của validator với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate không có lỗi.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Trường Prompt hợp lệ không nên gây ra lỗi validation.
    /// </summary>
    [Fact]
    public async Task Validate_ValidPrompt_ShouldNotReturnValidationError()
    {
        // Arrange
        var validPrompt = new string('a', 50); // Valid length
        var command = new GenerateNotificationTemplateContentCommand { Prompt = validPrompt };
        var validator = new GenerateNotificationTemplateContentCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
