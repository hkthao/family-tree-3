using backend.Application.Common.Constants;
using backend.Application.Prompts.Commands.CreatePrompt;
using backend.Application.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Prompts.Commands;

public class CreatePromptCommandHandlerTests : TestBase
{
    private readonly CreatePromptCommandHandler _handler;

    public CreatePromptCommandHandlerTests()
    {
        _handler = new CreatePromptCommandHandler(_context, _mockAuthorizationService.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreatePrompt_WhenUserIsAdmin()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        var command = new CreatePromptCommand
        {
            Code = "TEST_CODE_1",
            Title = "Test Title 1",
            Content = "Test Content 1",
            Description = "Test Description 1"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var prompt = await _context.Prompts.FindAsync(result.Value);
        prompt.Should().NotBeNull();
        prompt!.Code.Should().Be(command.Code);
        prompt.Title.Should().Be(command.Title);
        prompt.Content.Should().Be(command.Content);
        prompt.Description.Should().Be(command.Description);
    }

    [Fact]
    public async Task Handle_ShouldReturnAccessDenied_WhenUserIsNotAdmin()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);

        var command = new CreatePromptCommand
        {
            Code = "TEST_CODE_2",
            Title = "Test Title 2",
            Content = "Test Content 2",
            Description = "Test Description 2"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        _context.Prompts.Should().BeEmpty(); // No prompt should be created
    }
}
