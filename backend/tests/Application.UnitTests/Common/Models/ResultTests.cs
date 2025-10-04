using backend.Application.Common.Models;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Common.Models;

public class ResultTests
{
    // --- Non-generic Result Tests ---

    [Fact]
    public void NonGeneric_Success_ShouldReturnSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        result.ErrorSource.Should().BeNull();
    }

    [Fact]
    public void NonGeneric_Failure_ShouldReturnFailedResultWithDetails()
    {
        // Arrange
        var errorMessage = "Something went wrong.";
        var errorSource = "TestService.TestMethod";

        // Act
        var result = Result.Failure(errorMessage, errorSource);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(errorMessage);
        result.ErrorSource.Should().Be(errorSource);
    }

    // --- Generic Result<T> Tests ---

    [Fact]
    public void Generic_Success_ShouldReturnSuccessfulResultWithValue()
    {
        // Arrange
        var value = "Test Value";

        // Act
        var result = Result<string>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
        result.Error.Should().BeNull();
        result.ErrorSource.Should().BeNull();
    }

    [Fact]
    public void Generic_Failure_ShouldReturnFailedResultWithDetails()
    {
        // Arrange
        var errorMessage = "Data not found.";
        var errorSource = "GenericService.GetById";

        // Act
        var result = Result<int>.Failure(errorMessage, errorSource);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().Be(default(int)); // Default value for int
        result.Error.Should().Be(errorMessage);
        result.ErrorSource.Should().Be(errorSource);
    }
}
