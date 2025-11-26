using System.Text.Json;
using backend.Application.AI.DTOs; // PhotoAnalysisResultDto
using backend.Application.AI.Prompts; // PromptBuilder
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Memories.Commands.GenerateStory;
using backend.Application.Memories.DTOs; // GenerateStoryResponseDto
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Memories.Commands.GenerateStory;

public class GenerateStoryCommandTests : TestBase
{
    private readonly Mock<IN8nService> _n8nServiceMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;

    public GenerateStoryCommandTests() : base()
    {
        _n8nServiceMock = new Mock<IN8nService>();
        _authorizationServiceMock = new Mock<IAuthorizationService>(); // Mock AuthorizationService
    }

    private GenerateStoryCommandHandler CreateHandler()
    {
        return new GenerateStoryCommandHandler(
            _context,
            _authorizationServiceMock.Object,
            _n8nServiceMock.Object
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
        await _context.SaveChangesAsync();

        var command = new GenerateStoryCommand { MemberId = memberId, Style = "nostalgic", RawText = "User provided some context." };
        var handler = CreateHandler();

        _authorizationServiceMock.Setup(a => a.CanAccessFamily(familyId)).Returns(true);

        var generatedStoryResponse = new GenerateStoryResponseDto
        {
            Title = "A Nostalgic Tale",
            DraftStory = "This is a beautiful story...",
            Tags = new[] { "nostalgia", "childhood" },
            Keywords = new[] { "Nguyễn Văn A", "gia đình" }
        };

        _n8nServiceMock.Setup(s => s.CallChatWebhookAsync(
            It.IsAny<string>(), // sessionId
            It.IsAny<string>(), // message
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<string>.Success(JsonSerializer.Serialize(generatedStoryResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be(generatedStoryResponse.Title);
        result.Value.DraftStory.Should().Be(generatedStoryResponse.DraftStory);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nServiceFails()
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
        await _context.SaveChangesAsync();

        var command = new GenerateStoryCommand { MemberId = memberId, Style = "nostalgic", RawText = "User provided some context." };
        var handler = CreateHandler();

        _authorizationServiceMock.Setup(a => a.CanAccessFamily(familyId)).Returns(true);

        _n8nServiceMock.Setup(s => s.CallChatWebhookAsync(
            It.IsAny<string>(), // sessionId
            It.IsAny<string>(), // message
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<string>.Failure("N8n service error.", ErrorSources.ExternalServiceError));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("N8n service error.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nReturnsInvalidJson()
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
        await _context.SaveChangesAsync();

        var command = new GenerateStoryCommand { MemberId = memberId, Style = "nostalgic", RawText = "User provided some context." };
        var handler = CreateHandler();

        _authorizationServiceMock.Setup(a => a.CanAccessFamily(familyId)).Returns(true);

        _n8nServiceMock.Setup(s => s.CallChatWebhookAsync(
            It.IsAny<string>(), // sessionId
            It.IsAny<string>(), // message
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<string>.Success("{invalid json"));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tạo câu chuyện từ n8n trả về phản hồi không hợp lệ");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nReturnsEmptyResponse()
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
        await _context.SaveChangesAsync();

        var command = new GenerateStoryCommand { MemberId = memberId, Style = "nostalgic", RawText = "User provided some context." };
        var handler = CreateHandler();

        _authorizationServiceMock.Setup(a => a.CanAccessFamily(familyId)).Returns(true);

        _n8nServiceMock.Setup(s => s.CallChatWebhookAsync(
            It.IsAny<string>(), // sessionId
            It.IsAny<string>(), // message
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<string>.Success("")); // Empty response

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tạo câu chuyện từ n8n trả về phản hồi rỗng.");
    }
}
