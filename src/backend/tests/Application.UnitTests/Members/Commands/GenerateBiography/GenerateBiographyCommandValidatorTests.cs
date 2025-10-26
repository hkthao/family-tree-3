using AutoFixture;
using backend.Application.Members.Commands.GenerateBiography;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.GenerateBiography;

public class GenerateBiographyCommandValidatorTests
{
    private readonly GenerateBiographyCommandValidator _validator;
    private readonly IFixture _fixture;

    public GenerateBiographyCommandValidatorTests()
    {
        _validator = new GenerateBiographyCommandValidator();
        _fixture = new Fixture();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường MemberId của GenerateBiographyCommand là Guid.Empty.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GenerateBiographyCommand với MemberId được đặt là Guid.Empty.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính MemberId với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: MemberId là trường bắt buộc
    /// để xác định thành viên cần tạo tiểu sử và không được phép để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenMemberIdIsEmpty()
    {
        var command = _fixture.Build<GenerateBiographyCommand>()
            .With(c => c.MemberId, Guid.Empty)
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.MemberId)
            .WithErrorMessage("MemberId cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường Prompt của GenerateBiographyCommand
    /// vượt quá độ dài tối đa cho phép (1500 ký tự).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GenerateBiographyCommand với Prompt là một chuỗi dài hơn 1500 ký tự.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính Prompt với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Prompt có giới hạn độ dài tối đa
    /// để tránh việc gửi quá nhiều dữ liệu đến dịch vụ AI và đảm bảo hiệu suất.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenPromptExceedsMaxLength()
    {
        var longPrompt = new string('a', 1501);
        var command = _fixture.Build<GenerateBiographyCommand>()
            .With(c => c.Prompt, longPrompt)
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Prompt)
            .WithErrorMessage("Prompt must not exceed 1500 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi tất cả các trường
    /// của GenerateBiographyCommand đều hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GenerateBiographyCommand với tất cả các trường được điền đầy đủ và hợp lệ.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng không có bất kỳ lỗi validation nào được báo cáo.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Khi tất cả dữ liệu đầu vào tuân thủ
    /// các quy tắc validation, command phải được coi là hợp lệ và không có lỗi nào được trả về.
    /// </summary>
    [Fact]
    public void ShouldNotHaveErrorWhenAllFieldsAreValid()
    {
        var command = _fixture.Build<GenerateBiographyCommand>()
            .With(c => c.MemberId, Guid.NewGuid())
            .With(c => c.Prompt, "This is a valid prompt.")
            .With(c => c.Tone, BiographyTone.Emotional)
            .With(c => c.UseSystemData, true)
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
