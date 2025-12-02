using backend.Application.Identity.UserProfiles.Commands.UpdateUserProfile;
using FluentValidation.TestHelper;
using Xunit;

using backend.Application.Common.Constants; // New using
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
    public void ShouldHaveError_WhenAvatarBase64ExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi AvatarBase64 vượt quá ImageConstants.MaxAvatarBase64Length ký tự.
        var command = new UpdateUserProfileCommand
        {
            Name = "Valid Name",
            Email = "valid@example.com",
            AvatarBase64 = new string('a', ImageConstants.MaxAvatarBase64Length + 1)
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AvatarBase64)
              .WithErrorMessage($"AvatarBase64 must not exceed {ImageConstants.MaxAvatarBase64Length} characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenAvatarBase64IsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi AvatarBase64 là null.
        var command = new UpdateUserProfileCommand { AvatarBase64 = null };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarBase64);
    }
}
