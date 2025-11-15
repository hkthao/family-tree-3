using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Faces.Commands.SaveFaceLabels;
using backend.Application.Faces.Queries;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Faces.Commands.SaveFaceLabels;

public class SaveFaceLabelsTests : TestBase
{
    private readonly Mock<IN8nService> _mockN8nService;
    private readonly Mock<ILogger<SaveFaceLabelsCommandHandler>> _mockLogger;
    private readonly SaveFaceLabelsCommandHandler _handler;

    public SaveFaceLabelsTests()
    {
        _mockN8nService = new Mock<IN8nService>();
        _mockLogger = new Mock<ILogger<SaveFaceLabelsCommandHandler>>();
        _handler = new SaveFaceLabelsCommandHandler(_context, _mockLogger.Object, _mockN8nService.Object);
    }

    [Fact]
    public async Task Handle_ShouldSaveNewFaceLabelsAndCallN8nService()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var faceId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var faceLabels = new List<DetectedFaceDto>
        {
            new DetectedFaceDto
            {
                Id = faceId.ToString(),
                MemberId = memberId,
                Thumbnail = "base64thumb",
                Embedding = new List<double> { 0.1, 0.2 }
            }
        };

        _mockN8nService.Setup(s => s.CallEmbeddingWebhookAsync(It.IsAny<EmbeddingWebhookDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("N8n success"));

        var command = new SaveFaceLabelsCommand { ImageId = imageId, FaceLabels = faceLabels };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        var savedFace = _context.Faces.FirstOrDefault(f => f.Id == faceId);
        savedFace.Should().NotBeNull();
        savedFace!.MemberId.Should().Be(memberId);
        savedFace.Thumbnail.Should().Be("base64thumb");
        savedFace.Embedding.Should().BeEquivalentTo(new double[] { 0.1, 0.2 });

        _mockN8nService.Verify(s => s.CallEmbeddingWebhookAsync(
            It.Is<EmbeddingWebhookDto>(dto =>
                dto.EntityType == "Face" &&
                dto.EntityId == faceId.ToString() &&
                dto.ActionType == "SaveFaceEmbedding"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingFaceLabelsAndCallN8nService()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var existingMemberId = Guid.NewGuid();
        var faceId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        var existingFace = new Face { Id = faceId, MemberId = existingMemberId, Embedding = new List<double> { 0.5, 0.6 } };
        await _context.Faces.AddAsync(existingFace);
        await _context.SaveChangesAsync();

        var faceLabels = new List<DetectedFaceDto>
        {
            new DetectedFaceDto
            {
                Id = faceId.ToString(),
                MemberId = memberId, // New member ID
                Thumbnail = "updated_thumb",
                Embedding = new List<double> { 0.7, 0.8 } // Updated embedding
            }
        };

        _mockN8nService.Setup(s => s.CallEmbeddingWebhookAsync(It.IsAny<EmbeddingWebhookDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("N8n success"));

        var command = new SaveFaceLabelsCommand { ImageId = imageId, FaceLabels = faceLabels };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        var updatedFace = _context.Faces.FirstOrDefault(f => f.Id == faceId);
        updatedFace.Should().NotBeNull();
        updatedFace!.MemberId.Should().Be(memberId);
        updatedFace.Thumbnail.Should().Be("updated_thumb");
        updatedFace.Embedding.Should().BeEquivalentTo(new double[] { 0.7, 0.8 });

        _mockN8nService.Verify(s => s.CallEmbeddingWebhookAsync(
            It.Is<EmbeddingWebhookDto>(dto =>
                dto.EntityType == "Face" &&
                dto.EntityId == faceId.ToString() &&
                dto.ActionType == "SaveFaceEmbedding"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogN8nServiceFailureButContinueSaving()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var faceId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var faceLabels = new List<DetectedFaceDto>
        {
            new DetectedFaceDto
            {
                Id = faceId.ToString(),
                MemberId = memberId,
                Thumbnail = "base64thumb",
                Embedding = new List<double> { 0.1, 0.2 }
            }
        };

        _mockN8nService.Setup(s => s.CallEmbeddingWebhookAsync(It.IsAny<EmbeddingWebhookDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure("N8n service error"));

        var command = new SaveFaceLabelsCommand { ImageId = imageId, FaceLabels = faceLabels };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Operation should still succeed
        result.Value.Should().BeTrue();

        var savedFace = _context.Faces.FirstOrDefault(f => f.Id == faceId);
        savedFace.Should().NotBeNull(); // Face should still be saved

        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Failed to save face embedding for FaceId {faceId} to n8n: N8n service error")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWhenNoFaceLabelsProvided()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var faceLabels = new List<DetectedFaceDto>(); // Empty list
        var command = new SaveFaceLabelsCommand { ImageId = imageId, FaceLabels = faceLabels };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _context.Faces.Should().BeEmpty(); // No faces should be added
        _mockN8nService.Verify(s => s.CallEmbeddingWebhookAsync(
            It.IsAny<EmbeddingWebhookDto>(),
            It.IsAny<CancellationToken>()), Times.Never); // N8n service should not be called
    }
}
