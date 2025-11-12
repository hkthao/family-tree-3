using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Faces.Commands.DeleteFacesByMemberId;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using backend.Application.UnitTests.Common;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.Application.UnitTests.Faces.Commands.DeleteFacesByMemberId;

public class DeleteFacesByMemberIdTests : TestBase
{
    private readonly Mock<IN8nService> _mockN8nService;
    private readonly Mock<ILogger<DeleteFacesByMemberIdCommandHandler>> _mockLogger;
    private readonly DeleteFacesByMemberIdCommandHandler _handler;

    public DeleteFacesByMemberIdTests()
    {
        _mockN8nService = new Mock<IN8nService>();
        _mockLogger = new Mock<ILogger<DeleteFacesByMemberIdCommandHandler>>();
        _handler = new DeleteFacesByMemberIdCommandHandler(_context, _mockN8nService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteFacesAndCallN8nServiceSuccessfully()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var face1 = new Face { Id = Guid.NewGuid(), MemberId = memberId, Embedding = new List<double> { 1.0, 2.0 } };
        var face2 = new Face { Id = Guid.NewGuid(), MemberId = memberId, Embedding = new List<double> { 3.0, 4.0 } };
        await _context.Faces.AddAsync(face1);
        await _context.Faces.AddAsync(face2);
        await _context.SaveChangesAsync();

        _mockN8nService.Setup(s => s.CallEmbeddingWebhookAsync(It.IsAny<EmbeddingWebhookDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("N8n success"));

        var command = new DeleteFacesByMemberIdCommand(memberId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _context.Faces.Should().BeEmpty();
        _mockN8nService.Verify(s => s.CallEmbeddingWebhookAsync(
            It.Is<EmbeddingWebhookDto>(dto =>
                dto.EntityType == "Face" &&
                dto.EntityId == memberId.ToString() &&
                dto.ActionType == "DeleteFaceEmbedding"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogN8nServiceFailureButContinueDeletion()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var face1 = new Face { Id = Guid.NewGuid(), MemberId = memberId, Embedding = new List<double> { 1.0, 2.0 } };
        await _context.Faces.AddAsync(face1);
        await _context.SaveChangesAsync();

        _mockN8nService.Setup(s => s.CallEmbeddingWebhookAsync(It.IsAny<EmbeddingWebhookDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure("N8n service error"));

        var command = new DeleteFacesByMemberIdCommand(memberId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Operation should still succeed
        _context.Faces.Should().BeEmpty(); // Faces should still be deleted
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Failed to delete face embedding for MemberId {memberId}")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWhenNoFacesFound()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var command = new DeleteFacesByMemberIdCommand(memberId);

        _mockN8nService.Setup(s => s.CallEmbeddingWebhookAsync(It.IsAny<EmbeddingWebhookDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("N8n success"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _context.Faces.Should().BeEmpty(); // Ensure no faces were added or deleted
        _mockN8nService.Verify(s => s.CallEmbeddingWebhookAsync(
            It.IsAny<EmbeddingWebhookDto>(),
            It.IsAny<CancellationToken>()), Times.Once); // N8n service should still be called
    }
}
