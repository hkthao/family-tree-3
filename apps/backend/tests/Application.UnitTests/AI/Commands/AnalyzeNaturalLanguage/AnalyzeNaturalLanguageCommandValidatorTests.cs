using backend.Application.AI.Commands.AnalyzeNaturalLanguage;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.AI.Commands.AnalyzeNaturalLanguage;

public class AnalyzeNaturalLanguageCommandValidatorTests
{
    private readonly AnalyzeNaturalLanguageCommandValidator _validator;

    public AnalyzeNaturalLanguageCommandValidatorTests()
    {
        _validator = new AnalyzeNaturalLanguageCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenContentIsEmpty()
    {
        var command = new AnalyzeNaturalLanguageCommand { Content = "", SessionId = "testSessionId", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Content)
              .WithErrorMessage("Nội dung không được để trống.");
    }

    [Fact]
    public void ShouldHaveError_WhenContentIsTooLong()
    {
        var command = new AnalyzeNaturalLanguageCommand { Content = new string('a', 2001), SessionId = "testSessionId", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Content)
              .WithErrorMessage("Nội dung không được vượt quá 2000 ký tự.");
    }

    [Fact]
    public void ShouldHaveError_WhenSessionIdIsEmpty()
    {
        var command = new AnalyzeNaturalLanguageCommand { Content = "Some content", SessionId = "", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.SessionId)
              .WithErrorMessage("SessionId không được để trống.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        var command = new AnalyzeNaturalLanguageCommand { Content = "Some valid content", SessionId = "validSessionId", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
