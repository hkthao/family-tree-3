using backend.Application.Families.Commands.GenerateFamilyData;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.GenerateFamilyData;

public class GenerateFamilyDataCommandValidatorTests
{
    private readonly GenerateFamilyDataCommandValidator _validator;

    public GenerateFamilyDataCommandValidatorTests()
    {
        _validator = new GenerateFamilyDataCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenPromptIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Prompt là chuỗi rỗng.
        var command = new GenerateFamilyDataCommand(string.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Prompt)
              .WithErrorMessage("Prompt is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenPromptExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Prompt vượt quá 1000 ký tự.
        var command = new GenerateFamilyDataCommand(new string('a', 1001));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Prompt)
              .WithErrorMessage("Prompt must not exceed 1000 characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenPromptIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Prompt hợp lệ.
        var command = new GenerateFamilyDataCommand("Valid prompt");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Prompt);
    }
}
