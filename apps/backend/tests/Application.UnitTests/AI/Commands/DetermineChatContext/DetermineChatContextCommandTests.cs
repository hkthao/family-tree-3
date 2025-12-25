using backend.Application.AI.Commands.DetermineChatContext;
using backend.Application.AI.DTOs;
using backend.Application.AI.Enums;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Prompts.DTOs;
using backend.Application.Prompts.Queries.GetPromptById;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.AI.Commands.DetermineChatContext;

public class DetermineChatContextCommandTests
{
    private readonly Mock<IAiGenerateService> _mockAiGenerateService;
    private readonly Mock<ILogger<DetermineChatContextCommandHandler>> _mockLogger;
    private readonly Mock<IMediator> _mockMediator;
    private readonly DetermineChatContextCommandHandler _handler;
    private readonly DetermineChatContextCommandValidator _validator;

    public DetermineChatContextCommandTests()
    {
        _mockAiGenerateService = new Mock<IAiGenerateService>();
        _mockLogger = new Mock<ILogger<DetermineChatContextCommandHandler>>();
        _mockMediator = new Mock<IMediator>();

        _handler = new DetermineChatContextCommandHandler(_mockAiGenerateService.Object, _mockLogger.Object, _mockMediator.Object);
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
        _mockAiGenerateService.Verify(s => s.GenerateDataAsync<ContextClassificationDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Never);
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
        _mockAiGenerateService.Verify(s => s.GenerateDataAsync<ContextClassificationDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAiGenerateServiceFails()
    {
        // Arrange
        var command = new DetermineChatContextCommand { ChatMessage = "Test message", SessionId = "test_session" };
        _mockAiGenerateService.Setup(s => s.GenerateDataAsync<ContextClassificationDto>(
            It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ContextClassificationDto>.Failure("AI service error", "ExternalService"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI service error");
        result.ErrorSource.Should().Be("ExternalService");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAiReturnsNullValue()
    {
        // Arrange
        var command = new DetermineChatContextCommand { ChatMessage = "Test message", SessionId = "test_session" };
        _mockAiGenerateService.Setup(s => s.GenerateDataAsync<ContextClassificationDto>(
            It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ContextClassificationDto>.Success(null!)); // AI returns success but null value

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI trả về kết quả rỗng khi phân tích ngữ cảnh.");
        result.ErrorSource.Should().Be("AIError");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAiReturnsValidData()
    {
        // Arrange
        var command = new DetermineChatContextCommand { ChatMessage = "How to add a new member?", SessionId = "test_session" };
        var aiResponse = new ContextClassificationDto { Context = ContextType.QA, Reasoning = "User is asking a question about app usage." };
        _mockAiGenerateService.Setup(s => s.GenerateDataAsync<ContextClassificationDto>(
            It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ContextClassificationDto>.Success(aiResponse));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(aiResponse);
        _mockAiGenerateService.Verify(s => s.GenerateDataAsync<ContextClassificationDto>(
            It.Is<GenerateRequest>(req => req.ChatInput == command.ChatMessage && req.SessionId == command.SessionId && req.SystemPrompt == "System Prompt Content"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
