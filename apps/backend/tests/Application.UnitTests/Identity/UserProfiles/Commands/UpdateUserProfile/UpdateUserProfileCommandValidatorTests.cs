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
    public void ShouldHaveError_WhenIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Id là chuỗi rỗng.
        var command = new UpdateUserProfileCommand { };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id cannot be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenNameIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Name là chuỗi rỗng.
        var command = new UpdateUserProfileCommand { Name = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name cannot be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenNameExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Name vượt quá 256 ký tự.
        var command = new UpdateUserProfileCommand { Name = new string('a', 257) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must not exceed 256 characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenNameIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Name hợp lệ.
        var command = new UpdateUserProfileCommand { Name = "Valid Name" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveError_WhenEmailIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Email là null.
        var command = new UpdateUserProfileCommand { Email = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenEmailIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Email là chuỗi rỗng.
        var command = new UpdateUserProfileCommand { Email = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email cannot be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenEmailIsInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Email không hợp lệ.
        var command = new UpdateUserProfileCommand { Email = "invalid-email" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email must be a valid email address.");
    }

    [Fact]
    public void ShouldHaveError_WhenEmailExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Email vượt quá 256 ký tự.
        var command = new UpdateUserProfileCommand { Email = new string('a', 250) + "@example.com" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email must not exceed 256 characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenEmailIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Email hợp lệ.
        var command = new UpdateUserProfileCommand { Email = "test@example.com" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ShouldHaveError_WhenAvatarExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Avatar URL vượt quá 2048 ký tự.
        var command = new UpdateUserProfileCommand
        {

            Name = "Valid Name",
            Email = "valid@example.com",
            Avatar = new string('a', 2049)
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Avatar)
              .WithErrorMessage("Avatar URL must not exceed 2048 characters.");
    }

    [Fact]
    public void ShouldHaveError_WhenAvatarIsInvalidUrl()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Avatar URL không hợp lệ.
        var command = new UpdateUserProfileCommand { Avatar = "invalid-url" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Avatar)
              .WithErrorMessage("Avatar URL must be a valid URL.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenAvatarIsValidUrl()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Avatar URL hợp lệ.
        var command = new UpdateUserProfileCommand { Avatar = "http://example.com/avatar.png" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Avatar);
    }

    [Fact]
    public void ShouldNotHaveError_WhenAvatarIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Avatar là null.
        var command = new UpdateUserProfileCommand { Avatar = null };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Avatar);
    }
}
