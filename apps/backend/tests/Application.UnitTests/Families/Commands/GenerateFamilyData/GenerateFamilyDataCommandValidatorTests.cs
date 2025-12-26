using backend.Application.Families.Commands.GenerateFamilyData; // Updated using directive
using backend.Application.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.GenerateFamilyData; // Updated namespace

public class GenerateFamilyDataCommandValidatorTests : TestBase
{
    private readonly GenerateFamilyDataCommandValidator _validator; // Updated validator type

    public GenerateFamilyDataCommandValidatorTests()
    {
        _validator = new GenerateFamilyDataCommandValidator(); // Updated validator type
    }

    [Fact]
    public void ShouldHaveError_WhenFamilyIdIsEmpty()
    {
        // Arrange
        var command = new GenerateFamilyDataCommand { FamilyId = Guid.Empty, ChatInput = "Test" }; // Updated command type

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GenerateFamilyDataCommand.FamilyId));
    }

    [Fact]
    public void ShouldHaveError_WhenChatInputIsEmpty()
    {
        // Arrange
        var command = new GenerateFamilyDataCommand { FamilyId = Guid.NewGuid(), ChatInput = "" }; // Updated command type

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GenerateFamilyDataCommand.ChatInput));
    }

    [Fact]
    public void ShouldHaveError_WhenChatInputExceedsWordLimit()
    {
        // Arrange
        var longChatInput = string.Join(" ", Enumerable.Repeat("word", 201));
        var command = new GenerateFamilyDataCommand { FamilyId = Guid.NewGuid(), ChatInput = longChatInput }; // Updated command type

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GenerateFamilyDataCommand.ChatInput));
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new GenerateFamilyDataCommand { FamilyId = Guid.NewGuid(), ChatInput = "Valid chat input" }; // Updated command type

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
