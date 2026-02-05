using backend.Application.AI.Chat;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models.AppSetting; // Re-add this
using backend.Application.Knowledge; // NEW
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace backend.Application.UnitTests.AI.Commands.Chat;

public class ChatWithAssistantCommandHandlerTests
{
    private readonly Mock<ILogger<ChatWithAssistantCommandHandler>> _loggerMock;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IOptions<LLMGatewaySettings>> _mockLLMGatewaySettings;
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly Mock<IKnowledgeService> _mockKnowledgeService; // NEW

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
    }
}
