using backend.Application.Families.Commands.CreateFamily;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.CreateFamily;

public class CreateFamilyCommandValidatorTests
{
    private readonly CreateFamilyCommandValidator _validator;

    public CreateFamilyCommandValidatorTests()
    {
        _validator = new CreateFamilyCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenNameIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Name là null.
        var command = new CreateFamilyCommand { Name = null!, Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenNameIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Name là chuỗi rỗng.
        var command = new CreateFamilyCommand { Name = string.Empty, Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name cannot be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenNameExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Name vượt quá 200 ký tự.
        var command = new CreateFamilyCommand { Name = new string('a', 201), Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must not exceed 200 characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenNameIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Name hợp lệ.
        var command = new CreateFamilyCommand { Name = "Valid Name", Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveError_WhenDescriptionExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Description vượt quá 1000 ký tự.
        var command = new CreateFamilyCommand { Name = "Valid Name", Description = new string('a', 1001), Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description)
              .WithErrorMessage("Description must not exceed 1000 characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenDescriptionIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Description hợp lệ.
        var command = new CreateFamilyCommand { Name = "Valid Name", Description = "Valid description", Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ShouldHaveError_WhenAddressExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Address vượt quá 500 ký tự.
        var command = new CreateFamilyCommand { Name = "Valid Name", Address = new string('a', 501), Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Address)
              .WithErrorMessage("Address must not exceed 500 characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenAddressIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Address hợp lệ.
        var command = new CreateFamilyCommand { Name = "Valid Name", Address = "Valid address", Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Address);
    }

    [Fact]
    public void ShouldNotHaveError_WhenAvatarBase64IsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi AvatarBase64 hợp lệ.
        var validBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("some image data"));
        var command = new CreateFamilyCommand { Name = "Valid Name", AvatarBase64 = validBase64, Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarBase64);
    }

    [Fact]
    public void ShouldNotHaveError_WhenAvatarBase64IsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi AvatarBase64 là null.
        var command = new CreateFamilyCommand { Name = "Valid Name", AvatarBase64 = null, Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarBase64);
    }

    [Fact]
    public void ShouldNotHaveError_WhenAvatarBase64IsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi AvatarBase64 là chuỗi rỗng.
        var command = new CreateFamilyCommand { Name = "Valid Name", AvatarBase64 = string.Empty, Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarBase64);
    }

    [Fact]
    public void ShouldHaveError_WhenAvatarBase64IsInvalidFormat()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi AvatarBase64 không phải là định dạng Base64 hợp lệ.
        var command = new CreateFamilyCommand { Name = "Valid Name", AvatarBase64 = "invalid-base64-!@#", Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AvatarBase64)
              .WithErrorMessage("AvatarBase64 phải là một chuỗi Base64 hợp lệ hoặc rỗng.");
    }

    [Fact]
    public void ShouldHaveError_WhenAvatarBase64ExceedsSizeLimit()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi AvatarBase64 vượt quá giới hạn kích thước.
        // Khoảng 5MB bytes = 5 * 1024 * 1024 = 5242880 bytes
        // Kích thước Base64 ~ (kích thước byte * 4 / 3) + padding. 
        // Để vượt quá 5MB, chúng ta cần một chuỗi Base64 dài hơn một chút.
        var largeData = new byte[5 * 1024 * 1024 + 1]; // 5MB + 1 byte
        var largeBase64 = Convert.ToBase64String(largeData);

        var command = new CreateFamilyCommand { Name = "Valid Name", AvatarBase64 = largeBase64, Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AvatarBase64)
              .WithErrorMessage("File size exceeds the maximum limit of 5 MB.");
    }

    [Fact]
    public void ShouldHaveError_WhenVisibilityIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Visibility là null.
        var command = new CreateFamilyCommand { Name = "Valid Name", Visibility = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Visibility)
              .WithErrorMessage("Visibility cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenVisibilityIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Visibility là chuỗi rỗng.
        var command = new CreateFamilyCommand { Name = "Valid Name", Visibility = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Visibility)
              .WithErrorMessage("Visibility cannot be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenVisibilityIsInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Visibility không hợp lệ.
        var command = new CreateFamilyCommand { Name = "Valid Name", Visibility = "Invalid" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Visibility)
              .WithErrorMessage("Visibility must be 'Public' or 'Private'.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenVisibilityIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Visibility hợp lệ.
        var command = new CreateFamilyCommand { Name = "Valid Name", Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Visibility);

        command = new CreateFamilyCommand { Name = "Valid Name", Visibility = "Private" };
        result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Visibility);
    }
}
