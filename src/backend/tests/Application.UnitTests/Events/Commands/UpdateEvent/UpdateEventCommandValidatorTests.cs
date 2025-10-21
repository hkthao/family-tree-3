using backend.Application.Events.Commands.UpdateEvent;
using FluentValidation.TestHelper;
using Xunit;
using System;

namespace backend.Application.UnitTests.Events.Commands.UpdateEvent;

public class UpdateEventCommandValidatorTests
{
    private readonly UpdateEventCommandValidator _validator;

    public UpdateEventCommandValidatorTests()
    {
        _validator = new UpdateEventCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Id là Guid rỗng.
        var command = new UpdateEventCommand { Id = Guid.Empty, Name = "Valid Name", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenIdIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Id hợp lệ.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void ShouldHaveError_WhenNameIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Name là null.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = null!, FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenNameIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Name là chuỗi rỗng.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = string.Empty, FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name cannot be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenNameExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Name vượt quá 200 ký tự.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = new string('a', 201), FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must not exceed 200 characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenNameIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Name hợp lệ.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Event Name", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveError_WhenFamilyIdIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi FamilyId là null.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = null };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FamilyId)
              .WithErrorMessage("FamilyId cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenFamilyIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi FamilyId là Guid rỗng.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FamilyId)
              .WithErrorMessage("FamilyId cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenFamilyIdIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi FamilyId hợp lệ.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.FamilyId);
    }

    [Fact]
    public void ShouldHaveError_WhenDescriptionExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Description vượt quá 1000 ký tự.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid(), Description = new string('a', 1001) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description)
              .WithErrorMessage("Description must not exceed 1000 characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenDescriptionIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Description hợp lệ.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid(), Description = "Valid description" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ShouldHaveError_WhenLocationExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Location vượt quá 200 ký tự.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid(), Location = new string('a', 201) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Location)
              .WithErrorMessage("Location must not exceed 200 characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenLocationIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Location hợp lệ.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid(), Location = "Valid location" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Location);
    }

    [Fact]
    public void ShouldHaveError_WhenColorExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Color vượt quá 20 ký tự.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid(), Color = new string('a', 21) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Color)
              .WithErrorMessage("Color must not exceed 20 characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenColorIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Color hợp lệ.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid(), Color = "#FFFFFF" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Color);
    }

    [Fact]
    public void ShouldHaveError_WhenEndDateIsBeforeStartDate()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi EndDate trước StartDate.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid(), StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.EndDate)
              .WithErrorMessage("EndDate cannot be before StartDate.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenEndDateIsAfterStartDate()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi EndDate sau StartDate.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid(), StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void ShouldNotHaveError_WhenEndDateIsSameAsStartDate()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi EndDate bằng StartDate.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid(), StartDate = DateTime.Now, EndDate = DateTime.Now };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void ShouldNotHaveError_WhenStartDateOrEndDateIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi StartDate hoặc EndDate là null.
        var command1 = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid(), StartDate = null, EndDate = DateTime.Now };
        var result1 = _validator.TestValidate(command1);
        result1.ShouldNotHaveValidationErrorFor(x => x.EndDate);

        var command2 = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid(), StartDate = DateTime.Now, EndDate = null };
        var result2 = _validator.TestValidate(command2);
        result2.ShouldNotHaveValidationErrorFor(x => x.EndDate);

        var command3 = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid(), StartDate = null, EndDate = null };
        var result3 = _validator.TestValidate(command3);
        result3.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }
}
