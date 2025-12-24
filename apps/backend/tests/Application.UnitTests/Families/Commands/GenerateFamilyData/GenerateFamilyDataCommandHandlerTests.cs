using backend.Application.AI.DTOs;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Queries;
using backend.Application.Families.Commands.IncrementFamilyAiChatUsage;
using backend.Application.Families.Queries;
using backend.Application.Families.Commands.GenerateFamilyData;
using backend.Application.Members.Queries;
using backend.Application.Prompts.Queries.GetPromptById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using backend.Application.Prompts.DTOs; // ADDED: Missing using directive

namespace backend.Application.UnitTests.Families.Commands.GenerateFamilyData;

public class GenerateFamilyDataCommandHandlerTests : TestBase
{
    private readonly GenerateFamilyDataCommandHandler _handler;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IAiGenerateService> _mockAiGenerateService;
    private readonly Mock<ILogger<GenerateFamilyDataCommandHandler>> _mockLogger;

    public GenerateFamilyDataCommandHandlerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockAiGenerateService = new Mock<IAiGenerateService>();
        _mockLogger = new Mock<ILogger<GenerateFamilyDataCommandHandler>>();

        _handler = new GenerateFamilyDataCommandHandler(
            _context,
            _mockMediator.Object,
            _mockAiGenerateService.Object,
            _mockLogger.Object
        );

        // Setup default mediator behavior for prompt fetching
        _mockMediator.Setup(m => m.Send(
            It.Is<GetPromptByIdQuery>(q => q.Code == PromptConstants.UnifiedAiContentGenerationPromptCode),
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
        _mockAiGenerateService.Verify(s => s.GenerateDataAsync<CombinedAiContentDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnCombinedAiContentDto_WhenAiReturnsValidData()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var aiResponseDto = new CombinedAiContentDto
        {
            Families = new List<FamilyDto> { new FamilyDto { Id = familyId, Name = "Generated Family", Description = "Family description" } },
            Members = new List<MemberDto> { new MemberDto { Id = memberId, FirstName = "Generated", LastName = "Member", DateOfBirth = new DateTime(1990, 1, 1), DateOfDeath = new DateTime(2050, 1, 1) } },
            Events = new List<EventDto> { new EventDto { Id = eventId, Name = "Generated Event", Description = "Event description", SolarDate = new DateTime(2023, 1, 1) } }
        };

        _mockAiGenerateService.Setup(s => s.GenerateDataAsync<CombinedAiContentDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CombinedAiContentDto>.Success(aiResponseDto));

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
        result.Value.Should().BeEquivalentTo(aiResponseDto); // Direct comparison to the DTO

        _mockAiGenerateService.Verify(s => s.GenerateDataAsync<CombinedAiContentDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Send(It.IsAny<IncrementFamilyAiChatUsageCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAiServiceFails()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _mockAiGenerateService.Setup(s => s.GenerateDataAsync<CombinedAiContentDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CombinedAiContentDto>.Failure("AI service unavailable", ErrorSources.ExternalServiceError));

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
        result.Error.Should().Contain("AI service unavailable");
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
        _mockAiGenerateService.Verify(s => s.GenerateDataAsync<CombinedAiContentDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Send(It.IsAny<IncrementFamilyAiChatUsageCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUnifiedPromptNotFound()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _mockMediator.Setup(m => m.Send(
            It.Is<GetPromptByIdQuery>(q => q.Code == PromptConstants.UnifiedAiContentGenerationPromptCode),
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
        result.Error.Should().Contain($"System prompt for '{PromptConstants.UnifiedAiContentGenerationPromptCode}' not configured in the database.");
        result.ErrorSource.Should().Be(ErrorSources.InvalidConfiguration);
        _mockAiGenerateService.Verify(s => s.GenerateDataAsync<CombinedAiContentDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Never); // No AI call if prompt not found
        _mockMediator.Verify(m => m.Send(It.IsAny<IncrementFamilyAiChatUsageCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

