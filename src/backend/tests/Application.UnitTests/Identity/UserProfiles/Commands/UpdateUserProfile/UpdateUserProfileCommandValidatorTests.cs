using backend.Application.Identity.UserProfiles.Commands.UpdateUserProfile;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandValidatorTests
{
    private readonly UpdateUserProfileCommandValidator _validator;

    public UpdateUserProfileCommandValidatorTests()
    {
        _validator = new UpdateUserProfileCommandValidator();
    }

    [Fact]
    public void ShouldNotHaveValidationErrorForValidCommand()
    {
        var command = new UpdateUserProfileCommand
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test User",
            Email = "test@example.com",
            Avatar = "https://example.com/avatar.jpg"
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenIdIsNull()
    {
        var command = new UpdateUserProfileCommand { Id = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenIdIsEmpty()
    {
        var command = new UpdateUserProfileCommand { Id = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenNameIsNull()
    {
        var command = new UpdateUserProfileCommand { Name = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenNameIsEmpty()
    {
        var command = new UpdateUserProfileCommand { Name = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenNameExceedsMaxLength()
    {
        var command = new UpdateUserProfileCommand { Name = new string('a', 257) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenEmailIsNull()
    {
        var command = new UpdateUserProfileCommand { Email = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenEmailIsEmpty()
    {
        var command = new UpdateUserProfileCommand { Email = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenEmailIsInvalid()
    {
        var command = new UpdateUserProfileCommand { Email = "invalid-email" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenEmailExceedsMaxLength()
    {
        var command = new UpdateUserProfileCommand { Email = new string('a', 250) + "@example.com" }; // 250 + 10 = 260
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenAvatarUrlExceedsMaxLength()
    {
        var command = new UpdateUserProfileCommand
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Valid Name",
            Email = "valid@example.com",
            Avatar = "https://example.com/" + new string('a', 2025) + ".jpg" // Total length 2044, exceeds 2048
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Avatar);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenAvatarUrlIsInvalid()
    {
        var command = new UpdateUserProfileCommand { Avatar = "invalid-url" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Avatar);
    }
}
