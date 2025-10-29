using backend.Application.Events.Commands.GenerateEventData;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.GenerateEventData;

public class GenerateEventDataCommandValidatorTests
{
    private readonly GenerateEventDataCommandValidator _validator;

    public GenerateEventDataCommandValidatorTests()
    {
        _validator = new GenerateEventDataCommandValidator();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Prompt là chuỗi rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GenerateEventDataCommand với Prompt là chuỗi rỗng.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Prompt với thông báo lỗi "Prompt không được để trống.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Prompt là trường bắt buộc và không được để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenPromptIsEmpty()
    {
        // Arrange
        var command = new GenerateEventDataCommand(string.Empty);
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Prompt)
              .WithErrorMessage("Prompt không được để trống.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi Prompt hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GenerateEventDataCommand với Prompt hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính Prompt.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Prompt hợp lệ không nên gây ra lỗi xác thực.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenPromptIsValid()
    {
        // Arrange
        var command = new GenerateEventDataCommand("Valid prompt");
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Prompt);
    }
}
