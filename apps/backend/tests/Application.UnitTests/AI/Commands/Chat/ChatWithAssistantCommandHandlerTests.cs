using System.Net.Http; // Add this
using backend.Application.AI.Chat;
using backend.Application.AI.Commands.Chat.CallAiChatService;
using backend.Application.AI.Commands.DetermineChatContext;
using backend.Application.AI.DTOs;
using backend.Application.AI.Enums;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Common.Queries.ValidateUserAuthentication;
using backend.Application.Families.Commands.EnsureFamilyAiConfigExists;
using backend.Application.Families.Commands.GenerateFamilyData;
using backend.Application.Families.Commands.IncrementFamilyAiChatUsage;
using backend.Application.Families.Queries.CheckAiChatQuota;
using backend.Application.Prompts.DTOs;
using backend.Application.Prompts.Queries.GetPromptById;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.AI.Commands.Chat;

public class ChatWithAssistantCommandHandlerTestsV2
{
    private readonly Mock<ILogger<ChatWithAssistantCommandHandler>> _loggerMock;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IOptions<N8nSettings>> _mockN8nSettings;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory; // New field
    private readonly ChatWithAssistantCommandHandler _handler;

    public ChatWithAssistantCommandHandlerTestsV2()
    {
        _loggerMock = new Mock<ILogger<ChatWithAssistantCommandHandler>>();
        _mockMediator = new Mock<IMediator>();
        _mockN8nSettings = new Mock<IOptions<N8nSettings>>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>(); // Initialize new field

        // Setup N8nSettings mock
        _mockN8nSettings.Setup(o => o.Value).Returns(new N8nSettings
        {
            Chat = new ChatSettings { CollectionName = "DefaultChatCollection" }
        });

        _handler = new ChatWithAssistantCommandHandler(
            _loggerMock.Object,
            _mockMediator.Object,
            _mockN8nSettings.Object,
            _mockHttpClientFactory.Object // Pass new mock object
        );

        // --- Setup default mediator behaviors ---

        // ValidateUserAuthenticationQuery
        _mockMediator.Setup(m => m.Send(
            It.IsAny<ValidateUserAuthenticationQuery>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // EnsureFamilyAiConfigExistsCommand
        _mockMediator.Setup(m => m.Send(
            It.IsAny<EnsureFamilyAiConfigExistsCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // CheckAiChatQuotaQuery
        _mockMediator.Setup(m => m.Send(
            It.IsAny<CheckAiChatQuotaQuery>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // DetermineChatContextCommand
        _mockMediator.Setup(m => m.Send(
            It.IsAny<DetermineChatContextCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ContextClassificationDto>.Success(new ContextClassificationDto { Context = ContextType.QA })); // Default to QA

        // GetPromptByIdQuery for QA
        _mockMediator.Setup(m => m.Send(
            It.Is<GetPromptByIdQuery>(q => q.Code == PromptConstants.CHAT_QA_PROMPT),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PromptDto>.Success(new PromptDto { Content = "QA System Prompt Content" }));

        // GetPromptByIdQuery for FamilyDataLookup
        _mockMediator.Setup(m => m.Send(
            It.Is<GetPromptByIdQuery>(q => q.Code == PromptConstants.CHAT_FAMILY_DATA_LOOKUP_PROMPT),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PromptDto>.Success(new PromptDto { Content = "Family Data Lookup System Prompt Content" }));


        // IncrementFamilyAiChatUsageCommand
        _mockMediator.Setup(m => m.Send(
            It.IsAny<IncrementFamilyAiChatUsageCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // CallAiChatServiceCommand
        _mockMediator.Setup(m => m.Send(
            It.IsAny<CallAiChatServiceCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ChatResponse>.Success(new ChatResponse { Output = "AI Chat Response" }));

        // GenerateFamilyDataCommand
        _mockMediator.Setup(m => m.Send(
            It.IsAny<GenerateFamilyDataCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CombinedAiContentDto>.Success(new CombinedAiContentDto()));
    }

    // --- Orchestrator Tests ---

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { FamilyId = Guid.NewGuid(), SessionId = "s1", ChatInput = "hi" };
        _mockMediator.Setup(m => m.Send(
            It.IsAny<ValidateUserAuthenticationQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Unauthorized("Not authenticated", "Authentication"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be("Authentication");
        result.Error.Should().Be("Not authenticated");
        _mockMediator.Verify(m => m.Send(It.IsAny<EnsureFamilyAiConfigExistsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEnsureFamilyAiConfigFails()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { FamilyId = Guid.NewGuid(), SessionId = "s1", ChatInput = "hi" };
        _mockMediator.Setup(m => m.Send(
            It.IsAny<EnsureFamilyAiConfigExistsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Config error", "Config"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be("Config");
        _mockMediator.Verify(m => m.Send(It.IsAny<ValidateUserAuthenticationQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Send(It.IsAny<CheckAiChatQuotaQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnQuotaExceeded_WhenQuotaCheckFails()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { FamilyId = Guid.NewGuid(), SessionId = "s1", ChatInput = "hi" };
        _mockMediator.Setup(m => m.Send(
            It.IsAny<CheckAiChatQuotaQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Quota exceeded", "QuotaExceeded"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be("QuotaExceeded");
        _mockMediator.Verify(m => m.Send(It.IsAny<ValidateUserAuthenticationQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Send(It.IsAny<EnsureFamilyAiConfigExistsCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Send(It.IsAny<DetermineChatContextCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallAiChatService_WhenContextIsQA()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { FamilyId = Guid.NewGuid(), SessionId = "s1", ChatInput = "QA question" };
        _mockMediator.Setup(m => m.Send(
            It.IsAny<DetermineChatContextCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ContextClassificationDto>.Success(new ContextClassificationDto { Context = ContextType.QA }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Output.Should().Be("AI Chat Response");
        _mockMediator.Verify(m => m.Send(It.IsAny<CallAiChatServiceCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Send(It.IsAny<GenerateFamilyDataCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallGenerateFamilyData_WhenContextIsDataGeneration()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { FamilyId = Guid.NewGuid(), SessionId = "s1", ChatInput = "generate data" };
        _mockMediator.Setup(m => m.Send(
            It.IsAny<DetermineChatContextCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ContextClassificationDto>.Success(new ContextClassificationDto { Context = ContextType.DataGeneration }));
        _mockMediator.Setup(m => m.Send(
            It.IsAny<GenerateFamilyDataCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CombinedAiContentDto>.Success(new CombinedAiContentDto()));


        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Output.Should().Be("Dữ liệu gia đình đã được tạo thành công.");
        result.Value.GeneratedData.Should().NotBeNull();
        _mockMediator.Verify(m => m.Send(It.IsAny<CallAiChatServiceCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Send(It.IsAny<GenerateFamilyDataCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDataGenerationFails()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { FamilyId = Guid.NewGuid(), SessionId = "s1", ChatInput = "generate data" };
        _mockMediator.Setup(m => m.Send(
            It.IsAny<DetermineChatContextCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ContextClassificationDto>.Success(new ContextClassificationDto { Context = ContextType.DataGeneration }));
        _mockMediator.Setup(m => m.Send(
            It.IsAny<GenerateFamilyDataCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CombinedAiContentDto>.Failure("Data gen failed", "Generation"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be("Generation");
        _mockMediator.Verify(m => m.Send(It.IsAny<GenerateFamilyDataCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAppendLocationToChatInput_WhenContextIsDataGenerationAndLocationProvided()
    {
        // Arrange
        var initialChatInput = "original chat input";
        var location = new ChatLocation { Latitude = 10.0, Longitude = 20.0, Address = "Test Address" };
        var familyId = Guid.NewGuid();

        var command = new ChatWithAssistantCommand
        {
            FamilyId = familyId,
            SessionId = "s1",
            ChatInput = initialChatInput,
            Location = location
        };

        _mockMediator.Setup(m => m.Send(
            It.IsAny<DetermineChatContextCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ContextClassificationDto>.Success(new ContextClassificationDto { Context = ContextType.DataGeneration }));

        // Capture the GenerateFamilyDataCommand sent to mediator
        GenerateFamilyDataCommand? capturedCommand = null;
        _mockMediator.Setup(m => m.Send(
            It.IsAny<GenerateFamilyDataCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<CombinedAiContentDto>>, CancellationToken>((req, ct) => capturedCommand = (GenerateFamilyDataCommand)req)
            .ReturnsAsync(Result<CombinedAiContentDto>.Success(new CombinedAiContentDto()));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedCommand.Should().NotBeNull();
        capturedCommand!.FamilyId.Should().Be(familyId);
        capturedCommand!.ChatInput.Should().Contain(initialChatInput);
        capturedCommand!.ChatInput.Should().Contain($"--- Thông tin vị trí được cung cấp ---");
        capturedCommand!.ChatInput.Should().Contain($"[Location: Latitude={location.Latitude}, Longitude={location.Longitude}");
        // Address is optional, so check for it if present
        if (!string.IsNullOrWhiteSpace(location.Address))
        {
            capturedCommand.ChatInput.Should().Contain($", Address={location.Address}]");
        }
        else
        {
            capturedCommand.ChatInput.Should().Contain("]"); // Ensure the closing bracket is there
        }
        _mockMediator.Verify(m => m.Send(It.IsAny<GenerateFamilyDataCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldIncrementUsage_Always()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { FamilyId = Guid.NewGuid(), SessionId = "s1", ChatInput = "hi" };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockMediator.Verify(m => m.Send(It.IsAny<IncrementFamilyAiChatUsageCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenQAPromptFetchingFails()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { FamilyId = Guid.NewGuid(), SessionId = "s1", ChatInput = "QA question" };
        _mockMediator.Setup(m => m.Send(
            It.Is<GetPromptByIdQuery>(q => q.Code == PromptConstants.CHAT_QA_PROMPT),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PromptDto>.Failure("Prompt not found", "Configuration"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.InvalidConfiguration);
        result.Error.Should().Be("Không thể cấu hình hệ thống AI chat. Vui lòng liên hệ quản trị viên.");
        _mockMediator.Verify(m => m.Send(It.IsAny<CallAiChatServiceCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyDataLookupPromptFetchingFails()
    {
        // Arrange
        var command = new ChatWithAssistantCommand { FamilyId = Guid.NewGuid(), SessionId = "s1", ChatInput = "lookup family data" };
        _mockMediator.Setup(m => m.Send(
            It.IsAny<DetermineChatContextCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ContextClassificationDto>.Success(new ContextClassificationDto { Context = ContextType.FamilyDataLookup }));
        _mockMediator.Setup(m => m.Send(
            It.Is<GetPromptByIdQuery>(q => q.Code == PromptConstants.CHAT_FAMILY_DATA_LOOKUP_PROMPT),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PromptDto>.Failure("Prompt not found", "Configuration"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.InvalidConfiguration);
        result.Error.Should().Be("Không thể cấu hình hệ thống AI chat. Vui lòng liên hệ quản trị viên.");
        _mockMediator.Verify(m => m.Send(It.IsAny<CallAiChatServiceCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
