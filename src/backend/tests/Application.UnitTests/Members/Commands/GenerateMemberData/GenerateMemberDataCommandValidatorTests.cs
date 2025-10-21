using AutoFixture;
using backend.Application.Members.Commands.GenerateMemberData;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.GenerateMemberData;

public class GenerateMemberDataCommandValidatorTests
{
    private readonly GenerateMemberDataCommandValidator _validator;
    private readonly IFixture _fixture;

    public GenerateMemberDataCommandValidatorTests()
    {
        _validator = new GenerateMemberDataCommandValidator();
        _fixture = new Fixture();
    }

    [Fact]
    public void ShouldHaveErrorWhenPromptIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Prompt trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một GenerateMemberDataCommand với Prompt là chuỗi rỗng.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Prompt với thông báo phù hợp.
        var command = new GenerateMemberDataCommand(string.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Prompt)
            .WithErrorMessage("Prompt is required.");
        // 💡 Giải thích: Prompt là trường bắt buộc và không được để trống.
    }

    [Fact]
    public void ShouldHaveErrorWhenPromptExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Prompt vượt quá 1000 ký tự.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một GenerateMemberDataCommand với Prompt dài hơn 1000 ký tự.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Prompt với thông báo phù hợp.
        var longPrompt = _fixture.Create<string>();
        while (longPrompt.Length <= 1000)
        {
            longPrompt += _fixture.Create<string>();
        }
        longPrompt = longPrompt.Substring(0, 1001); // Đảm bảo chính xác 1001 ký tự

        var command = new GenerateMemberDataCommand(longPrompt);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Prompt)
            .WithErrorMessage("Prompt must not exceed 1000 characters.");
        // 💡 Giải thích: Prompt không được vượt quá 1000 ký tự.
    }
}
