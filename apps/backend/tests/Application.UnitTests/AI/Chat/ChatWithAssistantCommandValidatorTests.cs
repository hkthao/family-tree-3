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
    public void ShouldHaveError_WhenChatInputIsEmpty()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { SessionId = "testSession", ChatInput = "" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChatInput)
              .WithErrorMessage("ChatInput cannot be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenChatInputIsTooLong()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { SessionId = "testSession", ChatInput = new string('a', 2001) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChatInput)
              .WithErrorMessage("ChatInput must not exceed 2000 characters.");
    }

    [Fact]
    public void ShouldHaveError_WhenSessionIdIsEmpty()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { SessionId = "", ChatInput = "Valid message" };

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
        var command = new ChatWithAssistantCommand { SessionId = "testSession", ChatInput = "Valid message" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
