using backend.Application.Events.Commands.UpdateEvent;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.UpdateEvent;

public class UpdateEventCommandValidatorTests
{
    private readonly UpdateEventCommandValidator _validator;

    public UpdateEventCommandValidatorTests()
    {
        _validator = new UpdateEventCommandValidator();
    }

    [Fact]
    public void ShouldHaveErrorWhenNameIsEmpty()
    {
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveErrorWhenNameExceeds200Characters()
    {
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = new string('A', 201) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldNotHaveErrorWhenNameIsValid()
    {
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}
