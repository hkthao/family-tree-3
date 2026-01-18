using backend.Application.AI.Chat;
using backend.Application.AI.Commands.Chat.CallAiChatService;
using backend.Application.AI.Commands.DetermineChatContext;
using backend.Application.AI.DTOs;
using backend.Application.AI.Enums;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting; // Re-add this
using backend.Application.Common.Models.LLMGateway; // Add this for LLMGatewaySettings
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
using backend.Application.OCR.Commands; // NEW
using backend.Application.Files.DTOs; // NEW
using backend.Application.Knowledge; // NEW

namespace backend.Application.UnitTests.AI.Commands.Chat;

public class ChatWithAssistantCommandHandlerTests
{
    private readonly Mock<ILogger<ChatWithAssistantCommandHandler>> _loggerMock;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IOptions<LLMGatewaySettings>> _mockLLMGatewaySettings;
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly Mock<IKnowledgeService> _mockKnowledgeService; // NEW
    private readonly ChatWithAssistantCommandHandler _handler;

    public ChatWithAssistantCommandHandlerTests()
    {
        _loggerMock = new Mock<ILogger<ChatWithAssistantCommandHandler>>();
        _mockMediator = new Mock<IMediator>();
        _mockLLMGatewaySettings = new Mock<IOptions<LLMGatewaySettings>>();
        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _mockKnowledgeService = new Mock<IKnowledgeService>(); // NEW

        _mockLLMGatewaySettings.Setup(o => o.Value).Returns(new LLMGatewaySettings
        {
            LlmModel = "ollama:test-model"
        });

        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(true);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(It.IsAny<Guid>())).Returns(true);

        _handler = new ChatWithAssistantCommandHandler(
            _loggerMock.Object,
            _mockMediator.Object,
            _mockLLMGatewaySettings.Object,
            _mockAuthorizationService.Object,
            _mockKnowledgeService.Object // NEW
        );
    }
}
