using backend.Application.Families;
using backend.Application.Families.Commands.CreateFamilies;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.CreateFamilies;

public class CreateFamiliesCommandValidatorTests
{
    private readonly CreateFamiliesCommandValidator _validator;



    public CreateFamiliesCommandValidatorTests()
    {
        _validator = new CreateFamiliesCommandValidator(new FamilyDtoValidator());
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
        var validFamilyDto = new FamilyDto { Name = "Valid Family", Visibility = "Public" };
        var command = new CreateFamiliesCommand(new List<FamilyDto> { validFamilyDto });
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveError_WhenAnyFamilyDtoIsInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi bất kỳ FamilyDto nào trong danh sách không hợp lệ.
        var invalidFamilyDto = new FamilyDto { Name = string.Empty, Visibility = "Public" }; // Invalid name
        var validFamilyDto = new FamilyDto { Name = "Valid Family", Visibility = "Public" };

        var command = new CreateFamiliesCommand(new List<FamilyDto> { validFamilyDto, invalidFamilyDto });
        var result = _validator.TestValidate(command);
        result.Errors.Should().Contain(e => e.PropertyName == "Families[1].Name" && e.ErrorMessage == "Name is required.");
    }
}
