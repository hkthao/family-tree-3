using backend.Application.AI.Chat;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.AI.Chat;

public class ChatWithAssistantCommandHandlerTests
{
    private readonly Mock<IN8nService> _n8nServiceMock;
    private readonly ChatWithAssistantCommandHandler _handler;

    public ChatWithAssistantCommandHandlerTests()
    {
        _n8nServiceMock = new Mock<IN8nService>();
        _handler = new ChatWithAssistantCommandHandler(_n8nServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenN8nServiceReturnsSuccess()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { SessionId = "testSession", Message = "Hello AI" };
        var expectedResponse = "AI's response to Hello AI";
        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            command.SessionId, command.Message, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(expectedResponse));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedResponse);
        _n8nServiceMock.Verify(x => x.CallChatWebhookAsync(
            command.SessionId, command.Message, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nServiceReturnsFailure()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { SessionId = "testSession", Message = "Error message" };
        var expectedError = "Failed to communicate with N8n";
        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            command.SessionId, command.Message, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure(expectedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(expectedError);
        _n8nServiceMock.Verify(x => x.CallChatWebhookAsync(
            command.SessionId, command.Message, It.IsAny<CancellationToken>()), Times.Once);
    }
}
