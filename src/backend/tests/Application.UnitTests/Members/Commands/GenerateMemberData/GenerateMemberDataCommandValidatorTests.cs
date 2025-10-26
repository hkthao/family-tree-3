using backend.Application.Members.Commands.GenerateMemberData;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.GenerateMemberData;

public class GenerateMemberDataCommandValidatorTests
{
    private readonly GenerateMemberDataCommandValidator _validator;

    public GenerateMemberDataCommandValidatorTests()
    {
        _validator = new GenerateMemberDataCommandValidator();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường Prompt của GenerateMemberDataCommand là chuỗi rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GenerateMemberDataCommand với Prompt được đặt là chuỗi rỗng.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính Prompt với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Prompt là trường bắt buộc
    /// để cung cấp hướng dẫn cho việc tạo dữ liệu thành viên.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenPromptIsEmpty()
    {
        var command = new GenerateMemberDataCommand(string.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Prompt)
            .WithErrorMessage("Prompt is required.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường Prompt của GenerateMemberDataCommand
    /// vượt quá độ dài tối đa cho phép (1000 ký tự).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GenerateMemberDataCommand với Prompt là một chuỗi dài hơn 1000 ký tự.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính Prompt với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Prompt có giới hạn độ dài tối đa
    /// để tránh việc gửi quá nhiều dữ liệu đến dịch vụ AI và đảm bảo hiệu suất.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenPromptExceedsMaxLength()
    {
        var longPrompt = new string('a', 1001);
        var command = new GenerateMemberDataCommand(longPrompt);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Prompt)
            .WithErrorMessage("Prompt must not exceed 1000 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi trường Prompt của GenerateMemberDataCommand hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GenerateMemberDataCommand với Prompt là một chuỗi hợp lệ (không rỗng và không vượt quá độ dài tối đa).
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng không có bất kỳ lỗi validation nào được báo cáo.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Khi Prompt hợp lệ, command phải được coi là hợp lệ
    /// và không có lỗi nào được trả về.
    /// </summary>
    [Fact]
    public void ShouldNotHaveErrorWhenPromptIsValid()
    {
        var validPrompt = new string('a', 500);
        var command = new GenerateMemberDataCommand(validPrompt);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
