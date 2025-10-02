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
    public void ShouldHaveErrorWhenFirstNameIsEmpty()
    {
        var command = new CreateMemberCommand { FirstName = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void ShouldHaveErrorWhenFirstNameExceeds100Characters()
    {
        var command = new CreateMemberCommand { FirstName = new string('A', 101) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void ShouldNotHaveErrorWhenFirstNameIsValid()
    {
        var command = new CreateMemberCommand { FirstName = "Valid Name" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void ShouldHaveErrorWhenLastNameIsEmpty()
    {
        var command = new CreateMemberCommand { LastName = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void ShouldHaveErrorWhenLastNameExceeds100Characters()
    {
        var command = new CreateMemberCommand { LastName = new string('A', 101) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void ShouldNotHaveErrorWhenLastNameIsValid()
    {
        var command = new CreateMemberCommand { LastName = "Valid Name" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }
}
