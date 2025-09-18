using System;
using backend.Application.Common.Models;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Common.Models;

public class ResultTests
{
    [Fact]
    public void Success_ShouldReturnSucceededResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_ShouldReturnFailedResultWithErrors()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2" };

        // Act
        var result = Result.Failure(errors);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainInOrder(errors);
    }

    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var succeeded = true;
        var errors = new[] { "Error 1" };

        // Act
        var result = new Result(succeeded, errors);

        // Assert
        result.Succeeded.Should().Be(succeeded);
        result.Errors.Should().ContainInOrder(errors);
    }
}
