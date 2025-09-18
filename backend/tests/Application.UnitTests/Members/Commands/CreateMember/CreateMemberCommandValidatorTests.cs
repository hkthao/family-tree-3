using backend.Application.Members.Commands.CreateMember;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.CreateMember;

public class CreateMemberCommandValidatorTests
{
    private readonly CreateMemberCommandValidator _validator;

    public CreateMemberCommandValidatorTests()
    {
        _validator = new CreateMemberCommandValidator();
    }

    [Fact]
    public void ShouldHaveErrorWhenFullNameIsEmpty()
    {
        var command = new CreateMemberCommand { FullName = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void ShouldHaveErrorWhenFullNameExceeds200Characters()
    {
        var command = new CreateMemberCommand { FullName = new string('A', 201) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void ShouldNotHaveErrorWhenFullNameIsValid()
    {
        var command = new CreateMemberCommand { FullName = "Valid Name" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
    }
}
