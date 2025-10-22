using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;
using backend.Application.Members.Commands.GenerateMemberData;

namespace backend.Application.UnitTests.Members.Commands.GenerateMemberData;

public class GenerateMemberDataCommandValidatorTests
{
    private readonly GenerateMemberDataCommandValidator _validator;

    public GenerateMemberDataCommandValidatorTests()
    {
        _validator = new GenerateMemberDataCommandValidator();
    }

    [Fact]
    public void ShouldHaveErrorWhenPromptIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Prompt trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một GenerateMemberDataCommand với Prompt rỗng.
        // 2. Act: Gọi TestValidate trên validator.
        // 3. Assert: Kiểm tra có lỗi validation cho Prompt với thông báo phù hợp.
        var command = new GenerateMemberDataCommand(string.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Prompt);
                result.Errors.Should().Contain(e => e.ErrorMessage == "Prompt is required.");
                // 💡 Giải thích: Prompt là bắt buộc.
            }
        
            [Fact]
            public void ShouldHaveErrorWhenPromptExceedsMaxLength()
            {
                // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Prompt vượt quá 1000 ký tự.
                // ⚙️ Các bước (Arrange, Act, Assert):
                // 1. Arrange: Tạo một GenerateMemberDataCommand với Prompt dài hơn 1000 ký tự.
                // 2. Act: Gọi TestValidate trên validator.
                // 3. Assert: Kiểm tra có lỗi validation cho Prompt với thông báo phù hợp.
                var longPrompt = new string('a', 1001);
                var command = new GenerateMemberDataCommand(longPrompt);
        
                var result = _validator.TestValidate(command);
        
                result.ShouldHaveValidationErrorFor(x => x.Prompt);
                result.Errors.Should().Contain(e => e.ErrorMessage == "Prompt must not exceed 1000 characters.");
                // 💡 Giải thích: Prompt không được vượt quá 1000 ký tự.
            }
        
            [Fact]
            public void ShouldNotHaveErrorWhenPromptIsValid()
            {
                // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi Prompt hợp lệ.
                // ⚙️ Các bước (Arrange, Act, Assert):
                // 1. Arrange: Tạo một GenerateMemberDataCommand với Prompt hợp lệ.
                // 2. Act: Gọi TestValidate trên validator.
                // 3. Assert: Kiểm tra không có lỗi validation.
                var validPrompt = new string('a', 500);
                var command = new GenerateMemberDataCommand(validPrompt);
        
                var result = _validator.TestValidate(command);
        
                result.ShouldNotHaveAnyValidationErrors();
                // 💡 Giải thích: Command hợp lệ phải vượt qua validation.
            }
        }
        