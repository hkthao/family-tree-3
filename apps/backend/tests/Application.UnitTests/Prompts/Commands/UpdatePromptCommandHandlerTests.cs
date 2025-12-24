using backend.Application.Common.Constants;
using backend.Application.Prompts.Commands.UpdatePrompt;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Prompts.Commands;

public class UpdatePromptCommandHandlerTests : TestBase
{
    private readonly UpdatePromptCommandHandler _handler;

    public UpdatePromptCommandHandlerTests()
    {
        _handler = new UpdatePromptCommandHandler(_context, _mockAuthorizationService.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdatePrompt_WhenUserIsAdmin()
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

        var command = new UpdatePromptCommand
        {
            Id = existingPrompt.Id,
            Code = "UPDATED_CODE",
            Title = "Updated Title",
            Content = "Updated Content",
            Description = "Updated Description"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedPrompt = await _context.Prompts.FindAsync(existingPrompt.Id);
        updatedPrompt.Should().NotBeNull();
        updatedPrompt!.Code.Should().Be(command.Code);
        updatedPrompt.Title.Should().Be(command.Title);
        updatedPrompt.Content.Should().Be(command.Content);
        updatedPrompt.Description.Should().Be(command.Description);
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

        var command = new UpdatePromptCommand
        {
            Id = existingPrompt.Id,
            Code = "UPDATED_CODE",
            Title = "Updated Title",
            Content = "Updated Content",
            Description = "Updated Description"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);

        // Ensure prompt was not updated
        var originalPrompt = await _context.Prompts.FindAsync(existingPrompt.Id);
        originalPrompt!.Code.Should().Be(existingPrompt.Code);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenPromptDoesNotExist()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        var command = new UpdatePromptCommand
        {
            Id = Guid.NewGuid(), // Non-existent ID
            Code = "NON_EXISTENT",
            Title = "Non Existent",
            Content = "Non Existent",
            Description = "Non Existent"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Prompt not found.");
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }
}
