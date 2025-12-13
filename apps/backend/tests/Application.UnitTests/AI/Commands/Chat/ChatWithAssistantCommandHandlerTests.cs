using backend.Application.AI.Chat;
using backend.Application.AI.DTOs; // Add using directive for DTOs
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.AI.Chat;

public class ChatWithAssistantCommandHandlerTests
{
    private readonly Mock<IAiChatService> _chatAiServiceMock; // Changed to IAiChatService
    private readonly ChatWithAssistantCommandHandler _handler;

    public ChatWithAssistantCommandHandlerTests()
    {
        _chatAiServiceMock = new Mock<IAiChatService>(); // Changed to IAiChatService
        _handler = new ChatWithAssistantCommandHandler(_chatAiServiceMock.Object); // Pass IAiChatService mock
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAiChatServiceReturnsSuccess()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { SessionId = "testSession", ChatInput = "Hello AI", Metadata = new Dictionary<string, object>() }; // Changed Message to ChatInput, added Metadata
        var expectedResponse = "AI's response to Hello AI";

        _chatAiServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.Is<ChatRequest>(r => r.SessionId == command.SessionId && r.ChatInput == command.ChatInput), // Match the request object
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ChatResponse>.Success(new ChatResponse { Output = expectedResponse }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Output.Should().Be(expectedResponse);
        _chatAiServiceMock.Verify(x => x.CallChatWebhookAsync(
            It.Is<ChatRequest>(r => r.SessionId == command.SessionId && r.ChatInput == command.ChatInput), // Match the request object
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAiChatServiceReturnsFailure()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { SessionId = "testSession", ChatInput = "Error message", Metadata = new Dictionary<string, object>() }; // Changed Message to ChatInput, added Metadata
        var expectedError = "Failed to communicate with AI Chat Service";

        _chatAiServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.Is<ChatRequest>(r => r.SessionId == command.SessionId && r.ChatInput == command.ChatInput), // Match the request object
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ChatResponse>.Failure(expectedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(expectedError);
        _chatAiServiceMock.Verify(x => x.CallChatWebhookAsync(
            It.Is<ChatRequest>(r => r.SessionId == command.SessionId && r.ChatInput == command.ChatInput), // Match the request object
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
