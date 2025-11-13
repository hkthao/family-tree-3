
using backend.Application.Members.Commands.UpdateMemberBiography;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.UpdateMemberBiography;

public class UpdateMemberBiographyCommandValidatorTests
{
    private readonly UpdateMemberBiographyCommandValidator _validator;

    public UpdateMemberBiographyCommandValidatorTests()
    {
        _validator = new UpdateMemberBiographyCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenMemberIdIsEmpty()
    {
        var command = new UpdateMemberBiographyCommand { MemberId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.MemberId)
              .WithErrorMessage("MemberId cannot be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenBiographyContentExceedsMaxLength()
    {
        var command = new UpdateMemberBiographyCommand { MemberId = Guid.NewGuid(), BiographyContent = new string('a', 1501) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.BiographyContent)
              .WithErrorMessage("Biography content must not exceed 1500 characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        var command = new UpdateMemberBiographyCommand { MemberId = Guid.NewGuid(), BiographyContent = "Valid biography." };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
