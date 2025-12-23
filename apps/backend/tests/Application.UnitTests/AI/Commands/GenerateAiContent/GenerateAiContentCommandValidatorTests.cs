using System;
using backend.Application.AI.Commands.GenerateAiContent;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.AI.Commands.GenerateAiContent;

public class GenerateAiContentCommandValidatorTests
{
    private readonly GenerateAiContentCommandValidator _validator;

    public GenerateAiContentCommandValidatorTests()
    {
        _validator = new GenerateAiContentCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenFamilyIdIsEmpty()
    {
        var command = new GenerateAiContentCommand { FamilyId = Guid.Empty, ChatInput = "test", ContentType = "chat" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FamilyId)
              .WithErrorMessage("FamilyId không được để trống.");
    }

    [Fact]
    public void ShouldHaveError_WhenChatInputIsEmpty()
    {
        var command = new GenerateAiContentCommand { FamilyId = Guid.NewGuid(), ChatInput = "", ContentType = "chat" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ChatInput)
              .WithErrorMessage("ChatInput không được để trống.");
    }

    [Fact]
    public void ShouldHaveError_WhenChatInputExceeds200Words()
    {
        var longChatInput = string.Join(" ", Enumerable.Repeat("word", 201));
        var command = new GenerateAiContentCommand { FamilyId = Guid.NewGuid(), ChatInput = longChatInput, ContentType = "chat" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ChatInput)
              .WithErrorMessage("ChatInput không được vượt quá 200 từ.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenChatInputIsExactly200Words()
    {
        var chatInput = string.Join(" ", Enumerable.Repeat("word", 200));
        var command = new GenerateAiContentCommand { FamilyId = Guid.NewGuid(), ChatInput = chatInput, ContentType = "chat" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.ChatInput);
    }

    [Fact]
    public void ShouldHaveError_WhenContentTypeIsEmpty()
    {
        var command = new GenerateAiContentCommand { FamilyId = Guid.NewGuid(), ChatInput = "test", ContentType = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ContentType)
              .WithErrorMessage("ContentType không được để trống.");
    }

    [Fact]
    public void ShouldHaveError_WhenContentTypeIsInvalid()
    {
        var command = new GenerateAiContentCommand { FamilyId = Guid.NewGuid(), ChatInput = "test", ContentType = "invalidtype" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ContentType)
              .WithErrorMessage("ContentType không hợp lệ. Các loại hợp lệ là: family, member, event, chat.");
    }

    [Theory]
    [InlineData("family")]
    [InlineData("member")]
    [InlineData("event")]
    [InlineData("chat")]
    [InlineData("Family")] // Test case-insensitivity
    public void ShouldNotHaveError_WhenContentTypeIsValid(string contentType)
    {
        var command = new GenerateAiContentCommand { FamilyId = Guid.NewGuid(), ChatInput = "test", ContentType = contentType };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.ContentType);
    }

    [Fact]
    public void ShouldNotHaveAnyValidationErrors_WhenCommandIsValid()
    {
        var command = new GenerateAiContentCommand { FamilyId = Guid.NewGuid(), ChatInput = "valid input", ContentType = "chat" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
