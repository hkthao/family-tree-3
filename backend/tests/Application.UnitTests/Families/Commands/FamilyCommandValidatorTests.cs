using backend.Application.Families.Commands.CreateFamily;
using backend.Application.Families.Commands.UpdateFamily;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands;

public class FamilyCommandValidatorTests
{
    private readonly CreateFamilyCommandValidator _createValidator;
    private readonly UpdateFamilyCommandValidator _updateValidator;

    public FamilyCommandValidatorTests()
    {
        _createValidator = new CreateFamilyCommandValidator();
        _updateValidator = new UpdateFamilyCommandValidator();
    }

    // CreateFamilyCommandValidator Tests
    [Fact]
    public void CreateFamily_Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new CreateFamilyCommand { Name = string.Empty };
        var result = _createValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateFamily_Should_Have_Error_When_Name_Is_Too_Long()
    {
        var command = new CreateFamilyCommand { Name = new string('a', 201) };
        var result = _createValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateFamily_Should_Not_Have_Error_When_Name_Is_Valid()
    {
        var command = new CreateFamilyCommand { Name = "Valid Name" };
        var result = _createValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // UpdateFamilyCommandValidator Tests
    [Fact]
    public void UpdateFamily_Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = string.Empty };
        var result = _updateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void UpdateFamily_Should_Have_Error_When_Name_Is_Too_Long()
    {
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = new string('a', 201) };
        var result = _updateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void UpdateFamily_Should_Not_Have_Error_When_Name_Is_Valid()
    {
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "Valid Name" };
        var result = _updateValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
