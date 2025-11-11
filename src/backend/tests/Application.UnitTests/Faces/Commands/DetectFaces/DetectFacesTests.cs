using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Faces.Commands; // Added this line
using backend.Application.Faces.Commands.DetectFaces;
using backend.Application.Faces.Common;
using backend.Application.Faces.Queries;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.Faces.Commands.DetectFaces;

public class DetectFacesTests : TestBase
{
    private readonly Mock<IFaceApiService> _mockFaceApiService;
    private readonly Mock<IConfigProvider> _mockConfigProvider;
    private readonly Mock<ILogger<DetectFacesCommandHandler>> _mockLogger;
    private readonly Mock<IN8nService> _mockN8nService;
    private readonly DetectFacesCommandHandler _handler;

    public DetectFacesTests()
    {
        _mockFaceApiService = new Mock<IFaceApiService>();
        _mockConfigProvider = new Mock<IConfigProvider>();
        _mockN8nService = new Mock<IN8nService>();
        _mockLogger = new Mock<ILogger<DetectFacesCommandHandler>>();
        _handler = new DetectFacesCommandHandler(
            _mockFaceApiService.Object,
            _context,
            _mockConfigProvider.Object,
            _mockLogger.Object, // Passed ListLogger directly
            _mockN8nService.Object);
    }

    [Fact]
    public async Task Handle_ShouldDetectFacesAndReturnResponse()
    {
        // Arrange
        var imageBytes = new byte[] { 0x01, 0x02, 0x03 };
        var contentType = "image/jpeg";
        var faceApiResult = new List<FaceDetectionResultDto>
        {
            new FaceDetectionResultDto
            {
                Id = "face1",
                BoundingBox = new BoundingBoxDto { X = 1, Y = 2, Width = 3, Height = 4 },
                Confidence = 0.9f,
                Thumbnail = "base64thumb",
                Embedding = new double[] { 0.1, 0.2 }
            }
        };
        _mockFaceApiService.Setup(s => s.DetectFacesAsync(imageBytes, contentType, true))
            .Returns(Task.FromResult(faceApiResult));
        _mockN8nService.Setup(s => s.CallEmbeddingWebhookAsync(It.IsAny<EmbeddingWebhookDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure("No member found")); // Simulate no member found

        var command = new DetectFacesCommand { ImageBytes = imageBytes, ContentType = contentType, ReturnCrop = true };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.DetectedFaces.Should().ContainSingle();
        var detectedFace = result.Value.DetectedFaces.First();
        detectedFace.BoundingBox.X.Should().Be(1);
        detectedFace.Confidence.Should().Be(0.9f);
        detectedFace.Thumbnail.Should().Be("base64thumb");
        detectedFace.Embedding.Should().NotBeNull().And.HaveCount(2);
        detectedFace.MemberId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldHandleN8nServiceFailureGracefully()
    {
        // Arrange
        var imageBytes = new byte[] { 0x01, 0x02, 0x03 };
        var contentType = "image/jpeg";
        var faceApiResult = new List<FaceDetectionResultDto>
        {
            new FaceDetectionResultDto
            {
                Id = "face1",
                BoundingBox = new BoundingBoxDto { X = 1, Y = 2, Width = 3, Height = 4 },
                Confidence = 0.9f,
                Thumbnail = "base64thumb",
                Embedding = new double[] { 0.1, 0.2 }
            }
        };
        _mockFaceApiService.Setup(s => s.DetectFacesAsync(imageBytes, contentType, true))
            .Returns(Task.FromResult(faceApiResult));
        _mockN8nService.Setup(s => s.CallEmbeddingWebhookAsync(It.IsAny<EmbeddingWebhookDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure("N8n service error")); // Simulate n8n failure

        var command = new DetectFacesCommand { ImageBytes = imageBytes, ContentType = contentType, ReturnCrop = true };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Should still succeed overall
        result.Value.Should().NotBeNull();
        result.Value!.DetectedFaces.Should().ContainSingle();
        result.Value.DetectedFaces.First().MemberId.Should().BeNull(); // MemberId should be null
    }

    [Fact]
    public async Task Handle_ShouldHandleInvalidN8nResultGracefully()
    {
        // Arrange
        var imageBytes = new byte[] { 0x01, 0x02, 0x03 };
        var contentType = "image/jpeg";
        var faceApiResult = new List<FaceDetectionResultDto>
        {
            new FaceDetectionResultDto
            {
                Id = "face1",
                BoundingBox = new BoundingBoxDto { X = 1, Y = 2, Width = 3, Height = 4 },
                Confidence = 0.9f,
                Thumbnail = "base64thumb",
                Embedding = new double[] { 0.1, 0.2 }
            }
        };
        _mockFaceApiService.Setup(s => s.DetectFacesAsync(imageBytes, contentType, true))
            .Returns(Task.FromResult(faceApiResult));
        _mockN8nService.Setup(s => s.CallEmbeddingWebhookAsync(It.IsAny<EmbeddingWebhookDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("invalid-guid")); // Simulate invalid GUID from n8n

        var command = new DetectFacesCommand { ImageBytes = imageBytes, ContentType = contentType, ReturnCrop = true };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Should still succeed overall
        result.Value.Should().NotBeNull();
        result.Value!.DetectedFaces.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoFacesDetected()
    {
        // Arrange
        var imageBytes = new byte[] { 0x01, 0x02, 0x03 };
        var contentType = "image/jpeg";
        _mockFaceApiService.Setup(s => s.DetectFacesAsync(imageBytes, contentType, true))
            .Returns(Task.FromResult(new List<FaceDetectionResultDto>())); // Simulate no faces detected

        var command = new DetectFacesCommand { ImageBytes = imageBytes, ContentType = contentType, ReturnCrop = true };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.DetectedFaces.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnMemberDetailsWhenFound()
    {
        // Arrange
        var imageBytes = new byte[] { 0x01, 0x02, 0x03 };
        var contentType = "image/jpeg";
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Doe Family", Code = "DOE" };
        var member = new Member(memberId, "Doe", "John", "JD1", familyId, family);
        member.Update("John", "Doe", "JD1", null, null, new DateTime(1990, 1, 1), new DateTime(2050, 1, 1), null, null, null, null, null);

        await _context.Families.AddAsync(family);
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var faceApiResult = new List<FaceDetectionResultDto>
        {
            new FaceDetectionResultDto
            {
                Id = "face1",
                BoundingBox = new BoundingBoxDto { X = 1, Y = 2, Width = 3, Height = 4 },
                Confidence = 0.9f,
                Thumbnail = "base64thumb",
                Embedding = new double[] { 0.1, 0.2 }
            }
        };
        _mockFaceApiService.Setup(s => s.DetectFacesAsync(imageBytes, contentType, true))
            .Returns(Task.FromResult(faceApiResult));
        _mockN8nService.Setup(s => s.CallEmbeddingWebhookAsync(It.IsAny<EmbeddingWebhookDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(memberId.ToString())); // Simulate member found

        var command = new DetectFacesCommand { ImageBytes = imageBytes, ContentType = contentType, ReturnCrop = true };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.DetectedFaces.Should().ContainSingle();
        var detectedFace = result.Value.DetectedFaces.First();
        detectedFace.MemberId.Should().Be(memberId);
        detectedFace.MemberName.Should().Be("Doe John"); // Assuming FullName is "LastName FirstName"
        detectedFace.FamilyId.Should().Be(familyId);
        detectedFace.FamilyName.Should().Be("Doe Family");
        detectedFace.BirthYear.Should().Be(1990);
        detectedFace.DeathYear.Should().Be(2050);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureOnFaceApiServiceException()
    {
        // Arrange
        var imageBytes = new byte[] { 0x01, 0x02, 0x03 };
        var contentType = "image/jpeg";
        _mockFaceApiService.Setup(s => s.DetectFacesAsync(imageBytes, contentType, true))
            .ThrowsAsync(new Exception("Face API error")); // Simulate Face API exception

        var command = new DetectFacesCommand { ImageBytes = imageBytes, ContentType = contentType, ReturnCrop = true };
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("An unexpected error occurred: Face API error");
    }
}
