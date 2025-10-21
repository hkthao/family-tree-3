using backend.Application.Families.Commands.UpdateFamily;
using FluentValidation.TestHelper;
using Xunit;
using System;

namespace backend.Application.UnitTests.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandValidatorTests
{
    private readonly UpdateFamilyCommandValidator _validator;

    public UpdateFamilyCommandValidatorTests()
    {
        _validator = new UpdateFamilyCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Id là Guid rỗng.
        var command = new UpdateFamilyCommand { Id = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenIdIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Id hợp lệ.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "Valid Name", Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void ShouldHaveError_WhenNameIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Name là null.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = null!, Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenNameIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Name là chuỗi rỗng.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = string.Empty, Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name cannot be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenNameExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Name vượt quá 200 ký tự.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = new string('a', 201), Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must not exceed 200 characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenNameIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Name hợp lệ.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "Valid Family Name", Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveError_WhenDescriptionExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Description vượt quá 1000 ký tự.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Description = new string('a', 1001), Name = "Valid Name", Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description)
              .WithErrorMessage("Description must not exceed 1000 characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenDescriptionIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Description hợp lệ.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Description = "Valid description", Name = "Valid Name", Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ShouldHaveError_WhenAddressExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Address vượt quá 500 ký tự.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Address = new string('a', 501), Name = "Valid Name", Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Address)
              .WithErrorMessage("Address must not exceed 500 characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenAddressIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Address hợp lệ.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Address = "Valid address", Name = "Valid Name", Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Address);
    }

    [Fact]
    public void ShouldHaveError_WhenAvatarUrlExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi AvatarUrl vượt quá 2048 ký tự.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), AvatarUrl = "http://" + new string('a', 2048), Name = "Valid Name", Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AvatarUrl)
              .WithErrorMessage("AvatarUrl must not exceed 2048 characters.");
    }

    [Fact]
    public void ShouldHaveError_WhenAvatarUrlIsInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi AvatarUrl không phải là URL hợp lệ.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), AvatarUrl = "invalid-url", Name = "Valid Name", Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AvatarUrl)
              .WithErrorMessage("AvatarUrl must be a valid URL.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenAvatarUrlIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi AvatarUrl hợp lệ.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), AvatarUrl = "https://example.com/avatar.jpg", Name = "Valid Name", Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarUrl);
    }

    [Fact]
    public void ShouldNotHaveError_WhenAvatarUrlIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi AvatarUrl là rỗng.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), AvatarUrl = string.Empty, Name = "Valid Name", Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarUrl);
    }

    [Fact]
    public void ShouldHaveError_WhenVisibilityIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Visibility là null.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "Valid Name", Visibility = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Visibility)
              .WithErrorMessage("Visibility cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenVisibilityIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Visibility là chuỗi rỗng.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "Valid Name", Visibility = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Visibility)
              .WithErrorMessage("Visibility cannot be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenVisibilityIsInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Visibility không hợp lệ.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "Valid Name", Visibility = "Invalid" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Visibility)
              .WithErrorMessage("Visibility must be 'Public' or 'Private'.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenVisibilityIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Visibility hợp lệ.
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "Valid Name", Visibility = "Public" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Visibility);
    }
}
