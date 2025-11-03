
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
    public void ShouldHaveError_WhenIdIsEmpty()
    {
        var command = new UpdateMemberCommand { Id = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id cannot be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenLastNameIsNull()
    {
        var command = new UpdateMemberCommand { Id = Guid.NewGuid(), LastName = null!, FirstName = "John", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName)
              .WithErrorMessage("Last Name cannot be null.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        var command = new UpdateMemberCommand
        {
            Id = Guid.NewGuid(),
            LastName = "Doe",
            FirstName = "John",
            FamilyId = Guid.NewGuid()
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
