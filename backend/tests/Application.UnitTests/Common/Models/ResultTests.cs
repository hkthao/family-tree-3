using System;
using System.Collections.Generic;
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
        result.ErrorCode.Should().BeNull();
        result.Source.Should().BeNull();
    }

    [Fact]
    public void NonGeneric_Failure_ShouldReturnFailedResultWithDetails()
    {
        // Arrange
        var errorMessage = "Something went wrong.";
        var errorCode = 400;
        var source = "TestService.TestMethod";

        // Act
        var result = Result.Failure(errorMessage, errorCode, source);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(errorMessage);
        result.ErrorCode.Should().Be(errorCode);
        result.Source.Should().Be(source);
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
        result.ErrorCode.Should().BeNull();
        result.Source.Should().BeNull();
    }

    [Fact]
    public void Generic_Failure_ShouldReturnFailedResultWithDetails()
    {
        // Arrange
        var errorMessage = "Data not found.";
        var errorCode = 404;
        var source = "GenericService.GetById";

        // Act
        var result = Result<int>.Failure(errorMessage, errorCode, source);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().Be(default(int)); // Default value for int
        result.Error.Should().Be(errorMessage);
        result.ErrorCode.Should().Be(errorCode);
        result.Source.Should().Be(source);
    }
}