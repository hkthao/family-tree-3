
using backend.Application.Members.Commands.CreateMembers;
using backend.Application.Members.Queries;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.CreateMembers;

public class CreateMembersCommandValidatorTests
{
    private readonly CreateMembersCommandValidator _validator;

    public CreateMembersCommandValidatorTests()
    {
        _validator = new CreateMembersCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenMembersListIsEmpty()
    {
        var command = new CreateMembersCommand([]);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Members)
              .WithErrorMessage("At least one member is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenAnyMemberIsInvalid()
    {
        var invalidMember = new AIMemberDto { FirstName = "", LastName = "Doe", Gender = "Male", DateOfBirth = new DateTime(1990, 1, 1) };
        var command = new CreateMembersCommand(new List<AIMemberDto> { invalidMember });
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Members[0].FirstName");
    }

    [Fact]
    public void ShouldNotHaveError_WhenAllMembersAreValid()
    {
        var validMember = new AIMemberDto { FirstName = "John", LastName = "Doe", Gender = "Male", DateOfBirth = new DateTime(1990, 1, 1) };
        var command = new CreateMembersCommand(new List<AIMemberDto> { validMember });
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
