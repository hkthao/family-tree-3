using backend.Application.AI.Commands.GenerateAiContent;
using backend.Application.AI.DTOs;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Commands.IncrementFamilyAiChatUsage;
using backend.Application.Families.Queries; // ADDED
using backend.Application.Members.Queries; // ADDED
using backend.Application.Events.Queries; // ADDED
using backend.Application.Prompts.DTOs;
using backend.Application.Prompts.Queries.GetPromptById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using MediatR; // ADDED
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Dynamic;
using System.Text.Json;
using Xunit;
using Microsoft.EntityFrameworkCore; // For Include

namespace backend.Application.UnitTests.AI.Commands.GenerateAiContent;

public class GenerateAiContentCommandHandlerTests : TestBase
{
    private readonly GenerateAiContentCommandHandler _handler;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IAiGenerateService> _mockAiGenerateService;
    private readonly Mock<ILogger<GenerateAiContentCommandHandler>> _mockLogger;

    public GenerateAiContentCommandHandlerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockAiGenerateService = new Mock<IAiGenerateService>();
        _mockLogger = new Mock<ILogger<GenerateAiContentCommandHandler>>();

        _handler = new GenerateAiContentCommandHandler(
            _context,
            _mockMediator.Object,
            _mockAiGenerateService.Object,
            _mockLogger.Object
        );

        // Setup default mediator behavior for prompt fetching
        _mockMediator.Setup(m => m.Send(
            It.Is<GetPromptByIdQuery>(q => q.Code == PromptConstants.FamilyDataGenerationPromptCode),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PromptDto>.Success(new PromptDto { Content = "Family System Prompt" }));
        _mockMediator.Setup(m => m.Send(
            It.Is<GetPromptByIdQuery>(q => q.Code == PromptConstants.GenerateMemberBiographyPromptCode),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PromptDto>.Success(new PromptDto { Content = "Member System Prompt" }));
        _mockMediator.Setup(m => m.Send(
            It.Is<GetPromptByIdQuery>(q => q.Code == PromptConstants.GenerateEventDetailsPromptCode),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PromptDto>.Success(new PromptDto { Content = "Event System Prompt" }));
        _mockMediator.Setup(m => m.Send(
            It.Is<GetPromptByIdQuery>(q => q.Code == PromptConstants.AiAssistantChatPromptCode),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PromptDto>.Success(new PromptDto { Content = "Chat System Prompt" }));

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

        var command = new GenerateAiContentCommand
        {
            FamilyId = familyId,
            ChatInput = "Tell me about family",
            ContentType = "family"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Quota Exceeded");
        result.ErrorSource.Should().Be(ErrorSources.QuotaExceeded);
        _mockAiGenerateService.Verify(s => s.GenerateDataAsync<string>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnJsonExpandoObject_WhenAiReturnsValidJson_FamilyType()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = Family.Create("Test Family", "TF1", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var aiResponseDto = new FamilyDto { Id = familyId, Name = "Generated Family", Code = "GF1" };
        _mockAiGenerateService.Setup(s => s.GenerateDataAsync<FamilyDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FamilyDto>.Success(aiResponseDto));

        var command = new GenerateAiContentCommand
        {
            FamilyId = familyId,
            ChatInput = "Create a JSON object for a family",
            ContentType = "family"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeAssignableTo<ExpandoObject>();
        dynamic expandoResult = result.Value!;
        ((Guid)expandoResult.Id).Should().Be(familyId);
        ((string)expandoResult.Name).Should().Be("Generated Family");
        ((string)expandoResult.Code).Should().Be("GF1");
        _mockAiGenerateService.Verify(s => s.GenerateDataAsync<FamilyDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Send(It.IsAny<IncrementFamilyAiChatUsageCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnTextExpandoObject_WhenAiReturnsPlainText_ChatType()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = Family.Create("Test Family", "TF2", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var aiResponseText = "This is a plain text response from AI.";
        _mockAiGenerateService.Setup(s => s.GenerateDataAsync<string>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(aiResponseText));

        var command = new GenerateAiContentCommand
        {
            FamilyId = familyId,
            ChatInput = "Tell me a story",
            ContentType = "chat"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeAssignableTo<ExpandoObject>();
        dynamic expandoResult = result.Value!;
        ((string)expandoResult.Text).Should().Be(aiResponseText);
        _mockAiGenerateService.Verify(s => s.GenerateDataAsync<string>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Send(It.IsAny<IncrementFamilyAiChatUsageCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAiServiceFails()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = Family.Create("Test Family", "TF3", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        _mockAiGenerateService.Setup(s => s.GenerateDataAsync<FamilyDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FamilyDto>.Failure("AI service unavailable", ErrorSources.ExternalServiceError));

        var command = new GenerateAiContentCommand
        {
            FamilyId = familyId,
            ChatInput = "Generate something",
            ContentType = "family"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI service unavailable");
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
        _mockAiGenerateService.Verify(s => s.GenerateDataAsync<FamilyDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Send(It.IsAny<IncrementFamilyAiChatUsageCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithFallbackPrompt_WhenSpecificPromptNotFound()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = Family.Create("Test Family", "TF4", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        // Mock prompt not found for a specific code
        _mockMediator.Setup(m => m.Send(
            It.Is<GetPromptByIdQuery>(q => q.Code == PromptConstants.GenerateMemberBiographyPromptCode),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PromptDto>.NotFound("Prompt not found"));

        // Mock AI service to return a successful response with a default DTO when using fallback prompt
        _mockAiGenerateService.Setup(s => s.GenerateDataAsync<MemberDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<MemberDto>.Success(new MemberDto { FirstName = "Fallback", LastName = "Member" }));

        var command = new GenerateAiContentCommand
        {
            FamilyId = familyId,
            ChatInput = "Generate something",
            ContentType = "member"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeAssignableTo<ExpandoObject>();
        dynamic expandoResult = result.Value!;
        ((string)expandoResult.FirstName).Should().Be("Fallback");
        ((string)expandoResult.LastName).Should().Be("Member");

        _mockMediator.Verify(m => m.Send(It.IsAny<IncrementFamilyAiChatUsageCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockAiGenerateService.Verify(s => s.GenerateDataAsync<MemberDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenContentTypeIsInvalid()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = Family.Create("Test Family", "TF5", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var command = new GenerateAiContentCommand
        {
            FamilyId = familyId,
            ChatInput = "Generate something",
            ContentType = "invalidtype"
        };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid ContentType: invalidtype");

        _mockMediator.Verify(m => m.Send(It.IsAny<IncrementFamilyAiChatUsageCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockAiGenerateService.Verify(s => s.GenerateDataAsync<string>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Never); // No AI call for invalid content type
    }
}