using System.Text.Json; // NEW
using backend.Application.AI.Commands.DetermineChatContext;
using backend.Application.AI.DTOs;
using backend.Application.AI.Enums;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Services.LLMGateway; // NEW
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting; // NEW
using backend.Application.Common.Models.LLMGateway; // NEW
using backend.Application.Prompts.DTOs;
using backend.Application.Prompts.Queries.GetPromptById;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options; // NEW
using Moq;
using Xunit;

namespace backend.Application.UnitTests.AI.Commands.DetermineChatContext;

public class DetermineChatContextCommandTests
{
    private readonly Mock<ILLMGatewayService> _mockLLMGatewayService; // Changed from IAiGenerateService
    private readonly Mock<ILogger<DetermineChatContextCommandHandler>> _mockLogger;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IOptions<LLMGatewaySettings>> _mockLLMGatewaySettings; // NEW
    private readonly DetermineChatContextCommandHandler _handler;
    private readonly DetermineChatContextCommandValidator _validator;

    public DetermineChatContextCommandTests()
    {
        _mockLLMGatewayService = new Mock<ILLMGatewayService>(); // Changed
        _mockLogger = new Mock<ILogger<DetermineChatContextCommandHandler>>();
        _mockMediator = new Mock<IMediator>();
        _mockLLMGatewaySettings = new Mock<IOptions<LLMGatewaySettings>>(); // NEW

        // Setup LLMGatewaySettings mock
        _mockLLMGatewaySettings.Setup(o => o.Value).Returns(new LLMGatewaySettings
        {
            LlmModel = "ollama:test-context-model" // Provide a default model name for context
        });

        _handler = new DetermineChatContextCommandHandler(
            _mockLLMGatewayService.Object, // Changed
            _mockLogger.Object,
            _mockMediator.Object,
            _mockLLMGatewaySettings.Object); // NEW parameter
        _validator = new DetermineChatContextCommandValidator();

        // Setup default mediator behavior for prompt fetching
        _mockMediator.Setup(m => m.Send(
            It.Is<GetPromptByIdQuery>(q => q.Code == PromptConstants.CONTEXT_CLASSIFICATION_PROMPT),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PromptDto>.Success(new PromptDto { Content = "System Prompt Content" }));
    }

    // --- Validator Tests ---

    [Fact]
    public void Validator_ShouldHaveError_WhenChatMessageIsEmpty()
    {
        // Arrange
        var command = new DetermineChatContextCommand { ChatMessage = "", SessionId = "test_session" };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DetermineChatContextCommand.ChatMessage) && e.ErrorMessage == "Tin nhắn chat không được để trống.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenSessionIdIsEmpty()
    {
        // Arrange
        var command = new DetermineChatContextCommand { ChatMessage = "Hello", SessionId = "" };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DetermineChatContextCommand.SessionId) && e.ErrorMessage == "Session ID không được để trống.");
    }

    [Fact]
    public void Validator_ShouldNotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new DetermineChatContextCommand { ChatMessage = "Hello", SessionId = "test_session" };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // --- Command Handler Tests ---

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPromptFetchingFails()
    {
        // Arrange
        var command = new DetermineChatContextCommand { ChatMessage = "Test message", SessionId = "test_session" };
        _mockMediator.Setup(m => m.Send(
            It.Is<GetPromptByIdQuery>(q => q.Code == PromptConstants.CONTEXT_CLASSIFICATION_PROMPT),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PromptDto>.Failure("Prompt not found", "Configuration"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"System prompt for '{PromptConstants.CONTEXT_CLASSIFICATION_PROMPT}' not configured or fetched from the database.");
        result.ErrorSource.Should().Be("Configuration");
        _mockLLMGatewayService.Verify(s => s.GetChatCompletionAsync(It.IsAny<LLMChatCompletionRequest>(), It.IsAny<CancellationToken>()), Times.Never); // Changed
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPromptValueIsNull()
    {
        // Arrange
        var command = new DetermineChatContextCommand { ChatMessage = "Test message", SessionId = "test_session" };
        _mockMediator.Setup(m => m.Send(
            It.Is<GetPromptByIdQuery>(q => q.Code == PromptConstants.CONTEXT_CLASSIFICATION_PROMPT),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PromptDto>.Success(null!)); // Simulate null PromptDto value

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Nội dung system prompt cho '{PromptConstants.CONTEXT_CLASSIFICATION_PROMPT}' là null hoặc trống.");
        result.ErrorSource.Should().Be("Configuration");
        _mockLLMGatewayService.Verify(s => s.GetChatCompletionAsync(It.IsAny<LLMChatCompletionRequest>(), It.IsAny<CancellationToken>()), Times.Never); // Changed
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenLLMGatewayServiceFails() // Renamed test
    {
        // Arrange
        var command = new DetermineChatContextCommand { ChatMessage = "Test message", SessionId = "test_session" };
        _mockLLMGatewayService.Setup(s => s.GetChatCompletionAsync( // Changed
            It.IsAny<LLMChatCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LLMChatCompletionResponse>.Failure("LLM Gateway service error", "ExternalService")); // Changed

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("LLM Gateway service error");
        result.ErrorSource.Should().Be("ExternalService");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenLLMGatewayReturnsNullValue() // Renamed test
    {
        // Arrange
        var command = new DetermineChatContextCommand { ChatMessage = "Test message", SessionId = "test_session" };
        _mockLLMGatewayService.Setup(s => s.GetChatCompletionAsync( // Changed
            It.IsAny<LLMChatCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LLMChatCompletionResponse>.Success(null!)); // Changed: LLM Gateway returns success but null value

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("LLM Gateway trả về kết quả rỗng khi phân tích ngữ cảnh.");
        result.ErrorSource.Should().Be("AIError");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenLLMGatewayReturnsEmptyChoices() // NEW Test
    {
        // Arrange
        var command = new DetermineChatContextCommand { ChatMessage = "Test message", SessionId = "test_session" };
        var llmResponse = new LLMChatCompletionResponse
        {
            Id = "test-id",
            Object = "chat.completion",
            Choices = new List<LLMChatCompletionChoice>() // Empty choices
        };
        _mockLLMGatewayService.Setup(s => s.GetChatCompletionAsync(
            It.IsAny<LLMChatCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LLMChatCompletionResponse>.Success(llmResponse));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("LLM Gateway trả về kết quả rỗng khi phân tích ngữ cảnh.");
        result.ErrorSource.Should().Be("AIError");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenLLMGatewayReturnsInvalidJson() // NEW Test
    {
        // Arrange
        var command = new DetermineChatContextCommand { ChatMessage = "Test message", SessionId = "test_session" };
        var llmResponse = new LLMChatCompletionResponse
        {
            Id = "test-id",
            Object = "chat.completion",
            Choices = new List<LLMChatCompletionChoice>
            {
                new LLMChatCompletionChoice
                {
                    Index = 0,
                    Message = new LLMChatCompletionMessage { Role = "assistant", Content = "{invalid json" },
                    FinishReason = "stop"
                }
            }
        };
        _mockLLMGatewayService.Setup(s => s.GetChatCompletionAsync(
            It.IsAny<LLMChatCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LLMChatCompletionResponse>.Success(llmResponse));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Lỗi phân tích phản hồi của AI:");
        result.ErrorSource.Should().Be("AIError");
    }


    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenLLMGatewayReturnsValidData() // Renamed test
    {
        // Arrange
        var command = new DetermineChatContextCommand { ChatMessage = "How to add a new member?", SessionId = "test_session" };
        var aiResponse = new ContextClassificationDto { Context = ContextType.QA, Reasoning = "User is asking a question about app usage." };
        var llmResponse = new LLMChatCompletionResponse
        {
            Id = "test-id",
            Object = "chat.completion",
            Choices = new List<LLMChatCompletionChoice>
            {
                new LLMChatCompletionChoice
                {
                    Index = 0,
                    Message = new LLMChatCompletionMessage { Role = "assistant", Content = JsonSerializer.Serialize(aiResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) },
                    FinishReason = "stop"
                }
            }
        };

        _mockLLMGatewayService.Setup(s => s.GetChatCompletionAsync(
            It.IsAny<LLMChatCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LLMChatCompletionResponse>.Success(llmResponse));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(aiResponse);
        _mockLLMGatewayService.Verify(s => s.GetChatCompletionAsync(
            It.Is<LLMChatCompletionRequest>(req =>
                req.Model == "ollama:test-context-model" && // Check the model name
                req.Messages.Count == 2 &&
                req.Messages[0].Role == "system" &&
                req.Messages[0].Content == "System Prompt Content" &&
                req.Messages[1].Role == "user" &&
                req.Messages[1].Content == command.ChatMessage),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
