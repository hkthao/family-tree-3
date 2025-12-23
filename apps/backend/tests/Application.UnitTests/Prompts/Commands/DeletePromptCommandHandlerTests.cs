using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Prompts.Commands.DeletePrompt;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Prompts.Commands;

public class DeletePromptCommandHandlerTests : TestBase
{
    private readonly DeletePromptCommandHandler _handler;

    public DeletePromptCommandHandlerTests()
    {
        _handler = new DeletePromptCommandHandler(_context, _mockAuthorizationService.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeletePrompt_WhenUserIsAdmin()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        var existingPrompt = new Prompt
        {
            Code = "EXISTING_CODE",
            Title = "Existing Title",
            Content = "Existing Content",
            Description = "Existing Description"
        };
        _context.Prompts.Add(existingPrompt);
        await _context.SaveChangesAsync();

        var command = new DeletePromptCommand(existingPrompt.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var deletedPrompt = await _context.Prompts.FindAsync(existingPrompt.Id);
        deletedPrompt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnAccessDenied_WhenUserIsNotAdmin()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);

        var existingPrompt = new Prompt
        {
            Code = "EXISTING_CODE",
            Title = "Existing Title",
            Content = "Existing Content",
            Description = "Existing Description"
        };
        _context.Prompts.Add(existingPrompt);
        await _context.SaveChangesAsync();

        var command = new DeletePromptCommand(existingPrompt.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);

        // Ensure prompt was not deleted
        var prompt = await _context.Prompts.FindAsync(existingPrompt.Id);
        prompt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenPromptDoesNotExist()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        var command = new DeletePromptCommand(Guid.NewGuid()); // Non-existent ID

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Prompt not found.");
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }
}
