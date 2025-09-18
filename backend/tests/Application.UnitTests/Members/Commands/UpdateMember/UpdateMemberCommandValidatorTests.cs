using System;
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
    public void ShouldHaveErrorWhenFullNameIsEmpty()
    {
        var command = new UpdateMemberCommand { Id = Guid.NewGuid(), FullName = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void ShouldHaveErrorWhenFullNameExceeds200Characters()
    {
        var command = new UpdateMemberCommand { Id = Guid.NewGuid(), FullName = new string('A', 201) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void ShouldNotHaveErrorWhenFullNameIsValid()
    {
        var command = new UpdateMemberCommand { Id = Guid.NewGuid(), FullName = "Valid Name" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
    }
}
