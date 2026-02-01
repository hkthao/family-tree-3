using backend.Application.Common.Constants; // ADDED BACK
using backend.Application.Common.Interfaces.Services.LLMGateway;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting; // NEW
using backend.Application.Common.Models.LLMGateway; // NEW
using backend.Application.Families.Commands.GenerateFamilyData;
using backend.Application.Families.Commands.IncrementFamilyAiChatUsage;
using backend.Application.Prompts.DTOs;
using backend.Application.Prompts.Queries.GetPromptById;
using backend.Application.UnitTests.Common;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options; // NEW
using Moq;
using Xunit;


namespace backend.Application.UnitTests.Families.Commands.GenerateFamilyData;

public class GenerateFamilyDataCommandHandlerTests : TestBase
{
    private readonly GenerateFamilyDataCommandHandler _handler;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILLMGatewayService> _mockLlmGatewayService;
    private readonly Mock<ILogger<GenerateFamilyDataCommandHandler>> _mockLogger;
    private readonly Mock<IOptions<LLMGatewaySettings>> _mockLlmGatewaySettings; // NEW

    public GenerateFamilyDataCommandHandlerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLlmGatewayService = new Mock<ILLMGatewayService>();
        _mockLogger = new Mock<ILogger<GenerateFamilyDataCommandHandler>>();
        _mockLlmGatewaySettings = new Mock<IOptions<LLMGatewaySettings>>(); // NEW
        _mockLlmGatewaySettings.Setup(o => o.Value).Returns(new LLMGatewaySettings { LlmModel = "test-model" }); // NEW

        _handler = new GenerateFamilyDataCommandHandler(
            _mockMediator.Object,
            _mockLlmGatewayService.Object,
            _mockLogger.Object,
            _mockLlmGatewaySettings.Object // NEW
        );

        // Setup default mediator behavior for prompt fetching
        _mockMediator.Setup(m => m.Send(
            It.Is<GetPromptByIdQuery>(q => q.Code == PromptConstants.FAMILY_DATA_GENERATION_PROMPT),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PromptDto>.Success(new PromptDto { Content = "Unified System Prompt" }));

        // Setup default mediator behavior for AI usage increment
        _mockMediator.Setup(m => m.Send(
            It.IsAny<IncrementFamilyAiChatUsageCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenIncrementUsageFails()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _mockMediator.Setup(m => m.Send(
            It.IsAny<IncrementFamilyAiChatUsageCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Quota Exceeded", ErrorSources.QuotaExceeded));

        var command = new GenerateFamilyDataCommand
        {
            FamilyId = familyId,
            ChatInput = "Tell me about family"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Quota Exceeded");
        result.ErrorSource.Should().Be(ErrorSources.QuotaExceeded);
        _mockLlmGatewayService.Verify(s => s.GetChatCompletionAsync(It.IsAny<LLMChatCompletionRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnCombinedAiContentDto_WhenAiReturnsValidData()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var expectedCombinedContent = "Generated combined content";
        var llmResponse = new LLMChatCompletionResponse
        {
            Choices = new List<LLMChatCompletionChoice>
            {
                new LLMChatCompletionChoice { Message = new LLMChatCompletionMessage { Content = expectedCombinedContent } }
            }
        };

        _mockLlmGatewayService.Setup(s => s.GetChatCompletionAsync(It.IsAny<LLMChatCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LLMChatCompletionResponse>.Success(llmResponse));

        var command = new GenerateFamilyDataCommand
        {
            FamilyId = familyId,
            ChatInput = "Create a JSON object for family, member, and event"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.CombinedContent.Should().Be(expectedCombinedContent);

        _mockLlmGatewayService.Verify(s => s.GetChatCompletionAsync(It.IsAny<LLMChatCompletionRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Send(It.IsAny<IncrementFamilyAiChatUsageCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAiServiceFails()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _mockLlmGatewayService.Setup(s => s.GetChatCompletionAsync(It.IsAny<LLMChatCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LLMChatCompletionResponse>.Failure("LLM Gateway unavailable", ErrorSources.ExternalServiceError));

        var command = new GenerateFamilyDataCommand
        {
            FamilyId = familyId,
            ChatInput = "Generate something"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("LLM Gateway unavailable");
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
        _mockLlmGatewayService.Verify(s => s.GetChatCompletionAsync(It.IsAny<LLMChatCompletionRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Send(It.IsAny<IncrementFamilyAiChatUsageCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUnifiedPromptNotFound()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _mockMediator.Setup(m => m.Send(
            It.Is<GetPromptByIdQuery>(q => q.Code == PromptConstants.FAMILY_DATA_GENERATION_PROMPT),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PromptDto>.NotFound("Unified prompt not found"));

        var command = new GenerateFamilyDataCommand
        {
            FamilyId = familyId,
            ChatInput = "Generate something"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"System prompt for '{PromptConstants.FAMILY_DATA_GENERATION_PROMPT}' not configured in the database.");
        result.ErrorSource.Should().Be(ErrorSources.InvalidConfiguration);
        _mockLlmGatewayService.Verify(s => s.GetChatCompletionAsync(It.IsAny<LLMChatCompletionRequest>(), It.IsAny<CancellationToken>()), Times.Never); // No AI call if prompt not found
        _mockMediator.Verify(m => m.Send(It.IsAny<IncrementFamilyAiChatUsageCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

