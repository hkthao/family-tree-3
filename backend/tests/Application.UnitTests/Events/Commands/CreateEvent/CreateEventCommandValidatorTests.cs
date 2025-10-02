using backend.Application.Events.Commands.CreateEvent;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.CreateEvent;

public class CreateEventCommandValidatorTests
{
    private readonly CreateEventCommandValidator _validator;

    public CreateEventCommandValidatorTests()
    {
        _validator = new CreateEventCommandValidator();
    }

    [Fact]
    public void ShouldHaveErrorWhenNameIsEmpty()
    {
        var command = new CreateEventCommand { Name = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveErrorWhenNameExceeds200Characters()
    {
        var command = new CreateEventCommand { Name = new string('A', 201) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldNotHaveErrorWhenNameIsValid()
    {
        var command = new CreateEventCommand { Name = "Valid Name" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}
