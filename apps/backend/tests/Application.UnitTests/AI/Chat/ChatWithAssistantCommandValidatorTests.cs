using backend.Application.AI.Chat;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.AI.Chat;

public class ChatWithAssistantCommandValidatorTests
{
    private readonly ChatWithAssistantCommandValidator _validator;

    public ChatWithAssistantCommandValidatorTests()
    {
        _validator = new ChatWithAssistantCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenMessageIsEmpty()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { SessionId = "testSession", Message = "" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Message)
              .WithErrorMessage("Message cannot be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenMessageIsTooLong()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { SessionId = "testSession", Message = new string('a', 2001) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Message)
              .WithErrorMessage("Message must not exceed 2000 characters.");
    }

    [Fact]
    public void ShouldHaveError_WhenSessionIdIsEmpty()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { SessionId = "", Message = "Valid message" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SessionId)
              .WithErrorMessage("SessionId cannot be empty."); // Assuming SessionId cannot be empty, as per common practice
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { SessionId = "testSession", Message = "Valid message" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
