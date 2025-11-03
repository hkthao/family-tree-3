
using backend.Application.Relationships.Commands.GenerateRelationshipData;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.GenerateRelationshipData;

public class GenerateRelationshipDataCommandValidatorTests
{
    private readonly GenerateRelationshipDataCommandValidator _validator;

    public GenerateRelationshipDataCommandValidatorTests()
    {
        _validator = new GenerateRelationshipDataCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Prompt_Is_Empty()
    {
        var command = new GenerateRelationshipDataCommand(string.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Prompt);
    }

    [Fact]
    public void Should_Have_Error_When_Prompt_Exceeds_Max_Length()
    {
        var command = new GenerateRelationshipDataCommand(new string('a', 1001));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Prompt);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Prompt_Is_Valid()
    {
        var command = new GenerateRelationshipDataCommand("valid prompt");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Prompt);
    }
}
