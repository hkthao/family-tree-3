
using backend.Application.Members.Commands.DeleteMember;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.DeleteMember;

public class DeleteMemberCommandValidatorTests
{
    private readonly DeleteMemberCommandValidator _validator;

    public DeleteMemberCommandValidatorTests()
    {
        _validator = new DeleteMemberCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenIdIsEmpty()
    {
        var command = new DeleteMemberCommand(Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenIdIsValid()
    {
        var command = new DeleteMemberCommand(Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}
