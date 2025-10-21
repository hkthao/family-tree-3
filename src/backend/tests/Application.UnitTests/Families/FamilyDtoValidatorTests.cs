using backend.Application.Families;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Families;

public class FamilyDtoValidatorTests
{
    private readonly FamilyDtoValidator _validator;

    public FamilyDtoValidatorTests()
    {
        _validator = new FamilyDtoValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenNameIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Name là chuỗi rỗng.
        var familyDto = new FamilyDto { Name = string.Empty };
        var result = _validator.TestValidate(familyDto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenNameIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Name hợp lệ.
        var familyDto = new FamilyDto { Name = "Valid Family Name" };
        var result = _validator.TestValidate(familyDto);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveError_WhenVisibilityIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Visibility là null.
        var familyDto = new FamilyDto { Name = "Test Name", Visibility = null! };
        var result = _validator.TestValidate(familyDto);
        result.ShouldHaveValidationErrorFor(x => x.Visibility)
              .WithErrorMessage("Visibility is required.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenVisibilityIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Visibility hợp lệ.
        var familyDto = new FamilyDto { Name = "Test Name", Visibility = "Public" };
        var result = _validator.TestValidate(familyDto);
        result.ShouldNotHaveValidationErrorFor(x => x.Visibility);
    }
}
