using backend.Application.Members.Commands.UpdateMember;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.UpdateMember;

public class UpdateMemberCommandValidatorTests
{
    private readonly UpdateMemberCommandValidator _validator;

    public UpdateMemberCommandValidatorTests()
    {
        _validator = new UpdateMemberCommandValidator();
    }

    [Fact]
    public void ShouldHaveErrorWhenFirstNameIsEmpty()
    {
        var command = new UpdateMemberCommand { Id = Guid.NewGuid(), FirstName = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void ShouldHaveErrorWhenFirstNameExceeds100Characters()
    {
        var command = new UpdateMemberCommand { Id = Guid.NewGuid(), FirstName = new string('A', 101) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void ShouldNotHaveErrorWhenFirstNameIsValid()
    {
        var command = new UpdateMemberCommand { Id = Guid.NewGuid(), FirstName = "Valid Name" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void ShouldHaveErrorWhenLastNameIsEmpty()
    {
        var command = new UpdateMemberCommand { Id = Guid.NewGuid(), LastName = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void ShouldHaveErrorWhenLastNameExceeds100Characters()
    {
        var command = new UpdateMemberCommand { Id = Guid.NewGuid(), LastName = new string('A', 101) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void ShouldNotHaveErrorWhenLastNameIsValid()
    {
        var command = new UpdateMemberCommand { Id = Guid.NewGuid(), LastName = "Valid Name" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }
}
