using backend.Application.Common.Exceptions;
using FluentValidation.Results;
using Xunit;

namespace Application.UnitTests.Common.Exceptions;

public class ValidationExceptionTests
{
    [Fact]
    public void DefaultConstructor_ShouldCreateEmptyErrorDictionaryAndDefaultMessage()
    {
        // Arrange & Act
        var exception = new ValidationException();

        // Assert
        Assert.NotNull(exception.Errors);
        Assert.Empty(exception.Errors);
        Assert.Equal("One or more validation failures have occurred.", exception.Message);
    }

    [Fact]
    public void ConstructorWithFailures_ShouldPopulateErrorDictionaryCorrectly()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Property1", "Error1 for Property1"),
            new ValidationFailure("Property2", "Error1 for Property2"),
            new ValidationFailure("Property1", "Error2 for Property1"),
            new ValidationFailure("Property3", "Error1 for Property3"),
        };

        // Act
        var exception = new ValidationException(failures);

        // Assert
        Assert.NotNull(exception.Errors);
        Assert.Equal(3, exception.Errors.Count); // Property1, Property2, Property3

        Assert.True(exception.Errors.ContainsKey("Property1"));
        Assert.Equal(2, exception.Errors["Property1"].Length);
        Assert.Contains("Error1 for Property1", exception.Errors["Property1"]);
        Assert.Contains("Error2 for Property1", exception.Errors["Property1"]);

        Assert.True(exception.Errors.ContainsKey("Property2"));
        Assert.Single(exception.Errors["Property2"]);
        Assert.Contains("Error1 for Property2", exception.Errors["Property2"]);

        Assert.True(exception.Errors.ContainsKey("Property3"));
        Assert.Single(exception.Errors["Property3"]);
        Assert.Contains("Error1 for Property3", exception.Errors["Property3"]);
    }
}
