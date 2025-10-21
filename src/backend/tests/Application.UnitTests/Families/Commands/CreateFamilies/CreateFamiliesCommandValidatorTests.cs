using backend.Application.Families.Commands.CreateFamilies;
using backend.Application.Families;
using FluentValidation.TestHelper;
using Xunit;
using FluentValidation;
using FluentValidation.Results;
using FluentAssertions;

namespace backend.Application.UnitTests.Families.Commands.CreateFamilies;

public class CreateFamiliesCommandValidatorTests
{
    private readonly CreateFamiliesCommandValidator _validator;
    private readonly FamilyDtoValidator _familyDtoValidator;

    public CreateFamiliesCommandValidatorTests()
    {
        _familyDtoValidator = new FamilyDtoValidator();
        _validator = new CreateFamiliesCommandValidator(_familyDtoValidator);
    }

    [Fact]
    public void ShouldHaveError_WhenFamiliesListIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi danh sách Families rỗng.
        var command = new CreateFamiliesCommand(new List<FamilyDto>());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Families)
              .WithErrorMessage("At least one family is required.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenFamiliesListIsNotEmptyAndValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi danh sách Families không rỗng và hợp lệ.
        var command = new CreateFamiliesCommand(new List<FamilyDto> { new FamilyDto { Name = "Test Family", Visibility = "Public" } });
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveError_WhenAnyFamilyDtoIsInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi bất kỳ FamilyDto nào trong danh sách không hợp lệ.
        var invalidFamilyDto = new FamilyDto { Name = "" }; // Invalid name

        var command = new CreateFamiliesCommand(new List<FamilyDto> { invalidFamilyDto });
        var result = _validator.TestValidate(command);
        result.IsValid.Should().BeFalse(); // Overall validation should fail
        result.Errors.Should().Contain(e => e.PropertyName == "Families[0].Name" && e.ErrorMessage == "Name is required.");
    }
}
