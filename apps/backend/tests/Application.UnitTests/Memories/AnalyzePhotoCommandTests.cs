using backend.Application.Common.Constants;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Memories.Commands.AnalyzePhoto;
using backend.Application.Memories.DTOs;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;
using backend.Application.UnitTests.Common; // Added for TestBase
using backend.Infrastructure.Data; // For ApplicationDbContext
using System.Threading; // Added for CancellationToken
using System.Collections.Generic; // Added for List

namespace backend.Application.UnitTests.Memories.Commands;

public class AnalyzePhotoCommandTests : TestBase
{
    private readonly Mock<IOptions<N8nSettings>> _n8nSettingsMock;
    
    public AnalyzePhotoCommandTests() : base()
    {
        _n8nSettingsMock = new Mock<IOptions<N8nSettings>>();
    }

    private AnalyzePhotoCommandHandler CreateHandler(HttpClient httpClient, N8nSettings n8nSettings)
    {
        _n8nSettingsMock.Setup(x => x.Value).Returns(n8nSettings);

        return new AnalyzePhotoCommandHandler(
            _mapper,
            _context,
            _mockAuthorizationService.Object,
            httpClient,
            _n8nSettingsMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var handler = CreateHandler(httpClient, new N8nSettings { BaseUrl = "http://localhost:5678", PhotoAnalysisWebhook = "/webhook-test/photo-analysis" });

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

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var handler = CreateHandler(httpClient, new N8nSettings { BaseUrl = "http://localhost:5678", PhotoAnalysisWebhook = "/webhook-test/photo-analysis" });

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

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var handler = CreateHandler(httpClient, new N8nSettings { BaseUrl = "http://localhost:5678", PhotoAnalysisWebhook = "/webhook-test/photo-analysis" });

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

        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/webhook-test/photo-analysis")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(photoAnalysisResultDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            });

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
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var handler = CreateHandler(httpClient, new N8nSettings { BaseUrl = "http://localhost:5678", PhotoAnalysisWebhook = "" }); // Missing webhook config

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
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var handler = CreateHandler(httpClient, new N8nSettings { BaseUrl = "http://localhost:5678", PhotoAnalysisWebhook = "/webhook-test/photo-analysis" });

        var aiInput = new AiPhotoAnalysisInputDto
        {
            ImageBase64 = "base64image",
            Faces = new List<AiDetectedFaceDto>
            {
                new AiDetectedFaceDto { FaceId = "f1", Bbox = new List<int> { 10, 10, 20, 20 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.9 } }
            },
            TargetFaceId = "f1"
        };
        
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/webhook-test/photo-analysis")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError }); // Simulate failure

        var command = new AnalyzePhotoCommand { Input = aiInput };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Internal Server Error");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nWebhookReturnsEmptyResponse()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var handler = CreateHandler(httpClient, new N8nSettings { BaseUrl = "http://localhost:5678", PhotoAnalysisWebhook = "/webhook-test/photo-analysis" });

        var aiInput = new AiPhotoAnalysisInputDto
        {
            ImageBase64 = "base64image",
            Faces = new List<AiDetectedFaceDto>
            {
                new AiDetectedFaceDto { FaceId = "f1", Bbox = new List<int> { 10, 10, 20, 20 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.9 } }
            },
            TargetFaceId = "f1"
        };
        
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/webhook-test/photo-analysis")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("") // Empty response
            });

        var command = new AnalyzePhotoCommand { Input = aiInput };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Phân tích ảnh từ n8n trả về phản hồi không hợp lệ.");
    }
}