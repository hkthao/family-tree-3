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

    [Fact]
    public void ShouldHaveErrorWhenMemberIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi MemberId trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một GenerateBiographyCommand với MemberId là Guid.Empty.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính MemberId với thông báo phù hợp.
        var command = _fixture.Build<GenerateBiographyCommand>()
            .With(c => c.MemberId, Guid.Empty)
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.MemberId)
            .WithErrorMessage("MemberId cannot be empty.");
        // 💡 Giải thích: MemberId là trường bắt buộc và không được để trống.
    }

    [Fact]
    public void ShouldHaveErrorWhenPromptExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Prompt vượt quá độ dài tối đa.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một GenerateBiographyCommand với Prompt dài hơn 1500 ký tự.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Prompt với thông báo phù hợp.
        var longPrompt = new string('a', 1501);
        var command = _fixture.Build<GenerateBiographyCommand>()
            .With(c => c.Prompt, longPrompt)
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Prompt)
            .WithErrorMessage("Prompt must not exceed 1500 characters.");
        // 💡 Giải thích: Prompt có giới hạn độ dài tối đa là 1500 ký tự.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenAllFieldsAreValid()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi tất cả các trường đều hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một GenerateBiographyCommand với tất cả các trường hợp lệ.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng không có lỗi nào được báo cáo.
        var command = _fixture.Build<GenerateBiographyCommand>()
            .With(c => c.MemberId, Guid.NewGuid())
            .With(c => c.Prompt, "This is a valid prompt.")
            .With(c => c.Tone, BiographyTone.Emotional)
            .With(c => c.UseSystemData, true)
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
        // 💡 Giải thích: Khi tất cả các trường đều hợp lệ, validator không nên báo cáo lỗi nào.
    }
}
