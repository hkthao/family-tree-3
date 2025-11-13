
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
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var command = new DeleteRelationshipCommand(Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Id_Is_Valid()
    {
        var command = new DeleteRelationshipCommand(Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}
