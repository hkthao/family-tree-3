using backend.Application.Relationships.Commands.DeleteRelationship;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.DeleteRelationship;

public class DeleteRelationshipCommandValidatorTests
{
    private readonly DeleteRelationshipCommandValidator _validator;

    public DeleteRelationshipCommandValidatorTests()
    {
        _validator = new DeleteRelationshipCommandValidator();
    }

    [Fact]
    public void ShouldNotHaveValidationErrorForValidCommand()
    {
        var command = new DeleteRelationshipCommand(Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenIdIsEmpty()
    {
        var command = new DeleteRelationshipCommand(Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
