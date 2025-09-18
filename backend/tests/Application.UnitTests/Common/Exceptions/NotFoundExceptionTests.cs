using backend.Application.Common.Exceptions;
using Xunit;

namespace Application.UnitTests.Common.Exceptions;

public class NotFoundExceptionTests
{
    [Fact]
    public void DefaultConstructor_ShouldCreateException()
    {
        // Arrange & Act
        var exception = new NotFoundException();

        // Assert
        Assert.NotNull(exception);
        Assert.False(string.IsNullOrEmpty(exception.Message));
    }

    [Fact]
    public void ConstructorWithMessage_ShouldCreateExceptionWithCorrectMessage()
    {
        // Arrange
        var testMessage = "This is a test message.";

        // Act
        var exception = new NotFoundException(testMessage);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(testMessage, exception.Message);
    }

    [Fact]
    public void ConstructorWithMessageAndInnerException_ShouldCreateExceptionWithCorrectMessageAndInnerException()
    {
        // Arrange
        var testMessage = "This is a test message.";
        var innerException = new InvalidOperationException("Inner exception message.");

        // Act
        var exception = new NotFoundException(testMessage, innerException);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(testMessage, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void ConstructorWithNameAndKey_ShouldCreateExceptionWithFormattedMessage()
    {
        // Arrange
        var name = "Product";
        var key = 123;
        var expectedMessage = $"Entity \"{name}\" ({key}) was not found.";

        // Act
        var exception = new NotFoundException(name, key);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(expectedMessage, exception.Message);
    }
}
