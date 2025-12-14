using backend.Application.AI.DTOs; // For GenerateRequest
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemberStories.Commands.GenerateStory; // Updated
using backend.Application.MemberStories.DTOs; // GenerateStoryResponseDto // Updated
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.MemberStories.Commands.GenerateStory; // Updated

public class GenerateStoryCommandTests : TestBase
{
    private readonly Mock<IAiGenerateService> _aiGenerateServiceMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;

    public GenerateStoryCommandTests() : base()
    {
        _aiGenerateServiceMock = new Mock<IAiGenerateService>();
        _authorizationServiceMock = new Mock<IAuthorizationService>(); // Mock AuthorizationService
    }

    private GenerateStoryCommandHandler CreateHandler()
    {
        return new GenerateStoryCommandHandler(
            _context,
            _authorizationServiceMock.Object,
            _aiGenerateServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        // Arrange
        var command = new GenerateStoryCommand { MemberId = Guid.NewGuid() };
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(string.Format(ErrorMessages.NotFound, $"Member with ID {command.MemberId} not found."));
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAuthorizationDenied()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "FAMCODE" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();
        var member = new Member("Last", "First", "CODE", familyId, false);
        member.SetId(memberId);
        _context.Members.Add(member);

        // Add prompt for BuildSystemPrompt
        var promptCode = PromptConstants.StoryGenerationPromptCode;
        var promptEntity = new Prompt { Code = promptCode, Content = "Test Story Prompt", Title = "Story Generation Prompt" };
        _context.Prompts.Add(promptEntity);
        await _context.SaveChangesAsync();

        var command = new GenerateStoryCommand { MemberId = memberId };
        var handler = CreateHandler();

        _authorizationServiceMock.Setup(a => a.CanAccessFamily(familyId)).Returns(false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(ErrorMessages.AccessDenied);
    }

    [Fact]
    public async Task Handle_ShouldGenerateStorySuccessfully()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "FAMCODE" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();
        var member = new Member("Last", "First", "CODE", familyId, false);
        member.SetId(memberId);
        _context.Members.Add(member);

        // Add prompt for BuildSystemPrompt
        var promptCode = PromptConstants.StoryGenerationPromptCode;
        var promptEntity = new Prompt { Code = promptCode, Content = "Test Story Prompt", Title = "Story Generation Prompt" };
        _context.Prompts.Add(promptEntity);
        await _context.SaveChangesAsync();

        var command = new GenerateStoryCommand { MemberId = memberId, Style = "nostalgic", RawText = "User provided some context." };
        var handler = CreateHandler();

        _authorizationServiceMock.Setup(a => a.CanAccessFamily(familyId)).Returns(true);

        var generatedStoryResponse = new GenerateStoryResponseDto
        {
            Title = "A Nostalgic Tale",
            Story = "This is a beautiful story...",
            Tags = new[] { "nostalgia", "childhood" },
            Keywords = new[] { "Nguyễn Văn A", "gia đình" }
        };

        _aiGenerateServiceMock.Setup(s => s.GenerateDataAsync<GenerateStoryResponseDto>(
            It.IsAny<GenerateRequest>(), // Expect GenerateRequest
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<GenerateStoryResponseDto>.Success(generatedStoryResponse));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be(generatedStoryResponse.Title);
        result.Value.Story.Should().Be(generatedStoryResponse.Story);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAiServiceFails()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "FAMCODE" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();
        var member = new Member("Last", "First", "CODE", familyId, false);
        member.SetId(memberId);
        _context.Members.Add(member);

        // Add prompt for BuildSystemPrompt
        var promptCode = PromptConstants.StoryGenerationPromptCode;
        var promptEntity = new Prompt { Code = promptCode, Content = "Test Story Prompt", Title = "Story Generation Prompt" };
        _context.Prompts.Add(promptEntity);
        await _context.SaveChangesAsync();

        var command = new GenerateStoryCommand { MemberId = memberId, Style = "nostalgic", RawText = "User provided some context." };
        var handler = CreateHandler();

        _authorizationServiceMock.Setup(a => a.CanAccessFamily(familyId)).Returns(true);

        _aiGenerateServiceMock.Setup(s => s.GenerateDataAsync<GenerateStoryResponseDto>(
            It.IsAny<GenerateRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<GenerateStoryResponseDto>.Failure("AI service error.", ErrorSources.ExternalServiceError));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI service error.");
    }

    [Fact]
    public async Task BuildSystemPrompt_ShouldReturnCorrectPrompt()
    {
        // Arrange
        var promptCode = PromptConstants.StoryGenerationPromptCode;
        var expectedPromptContent = "Generate a compelling story based on the provided data.";
        var promptEntity = new Prompt { Code = promptCode, Content = expectedPromptContent, Title = "Story Generation Prompt" };
        _context.Prompts.Add(promptEntity);
        await _context.SaveChangesAsync();

        var handler = CreateHandler();

        // Use reflection to call the private method BuildSystemPrompt
        var method = typeof(GenerateStoryCommandHandler).GetMethod("BuildSystemPrompt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Should().NotBeNull(); // Ensure method was found

        // Act
        var invokedTask = method!.Invoke(handler, new object[] { Guid.NewGuid() }) as Task<string>;
        invokedTask.Should().NotBeNull(); // Ensure the invoked result is a Task<string>
        var result = await invokedTask!;

        // Assert
        result.Should().Be(expectedPromptContent);
    }
}
