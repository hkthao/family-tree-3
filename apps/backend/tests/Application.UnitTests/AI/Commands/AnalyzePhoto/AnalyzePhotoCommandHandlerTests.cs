using backend.Application.AI.Commands.AnalyzePhoto;
using backend.Application.AI.DTOs;
using backend.Application.AI.Prompts;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text.Json;
using Xunit;

namespace backend.Application.UnitTests.AI.Commands.AnalyzePhoto;

public class AnalyzePhotoCommandHandlerTests : TestBase
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IN8nService> _n8nServiceMock;
    private readonly AnalyzePhotoCommandHandler _handler;

    public AnalyzePhotoCommandHandlerTests() : base()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _n8nServiceMock = new Mock<IN8nService>();
        _handler = new AnalyzePhotoCommandHandler(_context, _authorizationServiceMock.Object, _n8nServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithAnalysisResult_WhenAllInputsAreValid()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var member = new Member(memberId, "John", "Doe", "JD1", familyId, new Family { Id = familyId, Name = "Test Family", Code = "TF1" }, false);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageUrl = "http://example.com/image.jpg",
                MemberInfo = new AiMemberInfoDto { Id = memberId.ToString() },
                Faces = new List<AiDetectedFaceDto> { new AiDetectedFaceDto { FaceId = "face1", Bbox = new List<int> { 0, 0, 10, 10 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.9 } } },
                TargetFaceId = "face1"
            }
        };

        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(true);

        var expectedPhotoAnalysisResult = new PhotoAnalysisResultDto
        {
            Summary = "A person smiling",
            Scene = "Outdoor",
            Event = "Birthday",
            Emotion = "Joy",
            YearEstimate = "2020",
            Objects = new List<string> { "cake" },
            Persons = new List<PhotoAnalysisPersonDto> { new PhotoAnalysisPersonDto { Id = "face1", Name = "John Doe", Emotion = "happy" } },
            Suggestions = new PhotoAnalysisSuggestionsDto { Scene = new List<string> { "park" } },
            CreatedAt = DateTime.UtcNow
        };
        var n8nResponseJson = JsonSerializer.Serialize(expectedPhotoAnalysisResult);

        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(n8nResponseJson));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Summary.Should().Be(expectedPhotoAnalysisResult.Summary);
        result.Value.Persons.Should().HaveCount(1);
        _n8nServiceMock.Verify(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.Is<string>(msg => msg.Contains("http://example.com/image.jpg")), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberIdFormatIsInvalid()
    {
        // Arrange
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageUrl = "http://example.com/image.jpg",
                MemberInfo = new AiMemberInfoDto { Id = "invalid-guid" }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid Member ID format");
        result.ErrorSource.Should().Be(ErrorSources.BadRequest);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberIsNotFound()
    {
        // Arrange
        var nonExistentMemberId = Guid.NewGuid();
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageUrl = "http://example.com/image.jpg",
                MemberInfo = new AiMemberInfoDto { Id = nonExistentMemberId.ToString() }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Member with ID {nonExistentMemberId} not found.");
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorizedToAccessFamily()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var member = new Member("John", "Doe", "JD1", familyId, false) { Id = memberId };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageUrl = "http://example.com/image.jpg",
                MemberInfo = new AiMemberInfoDto { Id = memberId.ToString() }
            }
        };

        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(false); // Not authorized

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nChatWebhookCallFails()
    {
        // Arrange
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageUrl = "http://example.com/image.jpg"
            }
        };

        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure("N8n service error", "ExternalError"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("N8n service error");
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nChatWebhookReturnsEmptyResponse()
    {
        // Arrange
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageUrl = "http://example.com/image.jpg"
            }
        };

        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(""));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Phân tích ảnh từ n8n trả về phản hồi rỗng.");
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nChatWebhookReturnsInvalidJson()
    {
        // Arrange
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageUrl = "http://example.com/image.jpg"
            }
        };

        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("{invalid json}"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Phân tích ảnh từ n8n trả về phản hồi không hợp lệ:");
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
    }
}
