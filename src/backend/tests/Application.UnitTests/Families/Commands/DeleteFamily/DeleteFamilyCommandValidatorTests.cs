using backend.Application.Families.Commands.DeleteFamily;
using FluentValidation.TestHelper;
using Xunit;
using System;

namespace backend.Application.UnitTests.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandValidatorTests
{
    private readonly DeleteFamilyCommandValidator _validator;

    public DeleteFamilyCommandValidatorTests()
    {
        _validator = new DeleteFamilyCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Id là Guid rỗng.
        var command = new DeleteFamilyCommand(Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenIdIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Id hợp lệ.
        var command = new DeleteFamilyCommand(Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}
