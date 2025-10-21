using backend.Application.Events.Commands.DeleteEvent;
using FluentValidation.TestHelper;
using Xunit;
using System;

namespace backend.Application.UnitTests.Events.Commands.DeleteEvent;

public class DeleteEventCommandValidatorTests
{
    private readonly DeleteEventCommandValidator _validator;

    public DeleteEventCommandValidatorTests()
    {
        _validator = new DeleteEventCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Id là Guid rỗng.
        var command = new DeleteEventCommand(Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenIdIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Id hợp lệ.
        var command = new DeleteEventCommand(Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}
