
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
    public void ShouldHaveError_WhenLastNameIsNull()
    {
        var command = new CreateMemberCommand { LastName = null!, FirstName = "John", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName)
              .WithErrorMessage("Last Name cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenFirstNameIsEmpty()
    {
        var command = new CreateMemberCommand { LastName = "Doe", FirstName = "", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("First Name cannot be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenFamilyIdIsEmpty()
    {
        var command = new CreateMemberCommand { LastName = "Doe", FirstName = "John", FamilyId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FamilyId)
              .WithErrorMessage("FamilyId cannot be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenDateOfDeathIsBeforeDateOfBirth()
    {
        var command = new CreateMemberCommand
        {
            LastName = "Doe",
            FirstName = "John",
            FamilyId = Guid.NewGuid(),
            DateOfBirth = new DateTime(2000, 1, 1),
            DateOfDeath = new DateTime(1999, 12, 31)
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DateOfDeath)
              .WithErrorMessage("DateOfDeath cannot be before DateOfBirth.");
    }

    [Fact]
    public void ShouldHaveError_WhenGenderIsInvalid()
    {
        var command = new CreateMemberCommand { LastName = "Doe", FirstName = "John", FamilyId = Guid.NewGuid(), Gender = "InvalidGender" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Gender)
              .WithErrorMessage("Gender must be 'Male', 'Female', or 'Other'.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        var command = new CreateMemberCommand
        {
            LastName = "Doe",
            FirstName = "John",
            FamilyId = Guid.NewGuid(),
            Gender = "Male"
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
