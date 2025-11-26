using System.Net;
using System.Text.Json;
using backend.Application.AI.Commands.AnalyzePhoto; // UPDATED USING
using backend.Application.AI.DTOs; // UPDATED USING
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces; // NEW
using backend.Application.Common.Models; // NEW
using backend.Application.Common.Models.AppSetting;
using backend.Application.UnitTests.Common; // Added for TestBase
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace backend.Application.UnitTests.Memories.Commands;

public class AnalyzePhotoCommandTests : TestBase
{
    private readonly Mock<IN8nService> _n8nServiceMock; // NEW
    private readonly Mock<IOptions<N8nSettings>> _n8nSettingsMock; // Keep for now, might be removed later

    public AnalyzePhotoCommandTests() : base()
    {
        _n8nServiceMock = new Mock<IN8nService>(); // NEW
        _n8nSettingsMock = new Mock<IOptions<N8nSettings>>();
    }

    private AnalyzePhotoCommandHandler CreateHandler() // Simplified parameters
    {
        return new AnalyzePhotoCommandHandler(
            _context,
            _mockAuthorizationService.Object,
            _n8nServiceMock.Object // Use IN8nService mock
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var handler = CreateHandler(); // Use simplified CreateHandler

        var command = new AnalyzePhotoCommand { Input = new AiPhotoAnalysisInputDto { MemberInfo = new AiMemberInfoDto { Id = memberId.ToString() } } };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(string.Format(ErrorMessages.NotFound, $"Member with ID {memberId} not found."));
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

        var handler = CreateHandler(); // Use simplified CreateHandler

        _mockAuthorizationService.Setup(a => a.CanAccessFamily(familyId)).Returns(false);

        var command = new AnalyzePhotoCommand { Input = new AiPhotoAnalysisInputDto { MemberInfo = new AiMemberInfoDto { Id = memberId.ToString() } } };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(ErrorMessages.AccessDenied);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenN8nWebhookRespondsSuccessfully()
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

        var aiInput = new AiPhotoAnalysisInputDto
        {
            ImageBase64 = "base64image",
            ImageSize = "512x512",
            Faces = new List<AiDetectedFaceDto>
            {
                new AiDetectedFaceDto { FaceId = "f1", Bbox = new List<int> { 10, 10, 20, 20 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.9 } }
            },
            TargetFaceId = "f1",
            MemberInfo = new AiMemberInfoDto { Id = memberId.ToString(), Name = "Test Member" }
        };

        var handler = CreateHandler(); // Use simplified CreateHandler

        _mockAuthorizationService.Setup(a => a.CanAccessFamily(familyId)).Returns(true);

        var photoAnalysisResultDto = new PhotoAnalysisResultDto
        {
            Summary = "A happy person in a park.",
            Scene = "Outdoor",
            Event = "Birthday",
            Emotion = "happy",
            YearEstimate = "2020s",
            Objects = new List<string> { "cake", "ball" },
            Persons = new List<PhotoAnalysisPersonDto> { new PhotoAnalysisPersonDto { Id = "p1", MemberId = memberId.ToString(), Name = "Test Member", Emotion = "happy" } },
            CreatedAt = DateTime.UtcNow
        };

        // Setup n8nServiceMock to return successful JSON string
        _n8nServiceMock.Setup(s => s.CallChatWebhookAsync(
            It.IsAny<string>(), // sessionId
            It.IsAny<string>(), // message
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<string>.Success(JsonSerializer.Serialize(photoAnalysisResultDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })));


        var command = new AnalyzePhotoCommand { Input = aiInput };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Emotion.Should().Be("happy");
        result.Value.Summary.Should().Be("A happy person in a park.");
        result.Value.CreatedAt.Should().NotBe(default(DateTime));
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nConfigMissing()
    {
        // Arrange
        var handler = CreateHandler(); // Use simplified CreateHandler

        var aiInput = new AiPhotoAnalysisInputDto
        {
            ImageBase64 = "base64image",
            Faces = new List<AiDetectedFaceDto>
            {
                new AiDetectedFaceDto { FaceId = "f1", Bbox = new List<int> { 10, 10, 20, 20 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.9 } }
            },
            TargetFaceId = "f1"
        };
        var command = new AnalyzePhotoCommand { Input = aiInput };

        // Setup n8nServiceMock to return failure for the chat webhook call
        _n8nServiceMock.Setup(s => s.CallChatWebhookAsync(
            It.IsAny<string>(), // sessionId
            It.IsAny<string>(), // message
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<string>.Failure("N8n configuration for photo analysis is missing.")); // Simulate config missing

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("N8n configuration for photo analysis is missing.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nWebhookFails()
    {
        // Arrange
        var handler = CreateHandler(); // Use simplified CreateHandler

        var aiInput = new AiPhotoAnalysisInputDto
        {
            ImageBase64 = "base64image",
            Faces = new List<AiDetectedFaceDto>
            {
                new AiDetectedFaceDto { FaceId = "f1", Bbox = new List<int> { 10, 10, 20, 20 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.9 } }
            },
            TargetFaceId = "f1"
        };

        // Setup n8nServiceMock to simulate webhook failure
        _n8nServiceMock.Setup(s => s.CallChatWebhookAsync(
            It.IsAny<string>(), // sessionId
            It.IsAny<string>(), // message
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<string>.Failure("N8n service returned internal server error.", ErrorSources.ExternalServiceError)); // Simulate failure


        var command = new AnalyzePhotoCommand { Input = aiInput };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("N8n service returned internal server error."); // Updated expected error message
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nWebhookReturnsEmptyResponse()
    {
        // Arrange
        var handler = CreateHandler(); // Use simplified CreateHandler

        var aiInput = new AiPhotoAnalysisInputDto
        {
            ImageBase64 = "base64image",
            Faces = new List<AiDetectedFaceDto>
            {
                new AiDetectedFaceDto { FaceId = "f1", Bbox = new List<int> { 10, 10, 20, 20 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.9 } }
            },
            TargetFaceId = "f1"
        };

        // Setup n8nServiceMock to simulate an empty response
        _n8nServiceMock.Setup(s => s.CallChatWebhookAsync(
            It.IsAny<string>(), // sessionId
            It.IsAny<string>(), // message
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<string>.Success("")); // Empty response


        var command = new AnalyzePhotoCommand { Input = aiInput };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Phân tích ảnh từ n8n trả về phản hồi rỗng.");
    }
}
