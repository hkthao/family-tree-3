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

    [Fact]
    public void ShouldHaveError_WhenPromptIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Prompt là chuỗi rỗng.
        var command = new GenerateEventDataCommand(string.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Prompt)
              .WithErrorMessage("Prompt không được để trống.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenPromptIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Prompt hợp lệ.
        var command = new GenerateEventDataCommand("Valid prompt");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Prompt);
    }
}
