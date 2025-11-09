using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Faces.Commands.SaveFaceLabels;
using backend.Application.Faces.Common;
using backend.Application.Faces.Queries;
using backend.Application.UnitTests.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Faces.Commands.SaveFaceLabels
{
    public class SaveFaceLabelsCommandHandlerTests : TestBase
    {
        private readonly Mock<IConfigProvider> _configProviderMock;
        private readonly Mock<ILogger<SaveFaceLabelsCommandHandler>> _loggerMock;
        private readonly Mock<IN8nService> _n8nServiceMock;

        public SaveFaceLabelsCommandHandlerTests()
        {
            _configProviderMock = new Mock<IConfigProvider>();
            _loggerMock = new Mock<ILogger<SaveFaceLabelsCommandHandler>>();
            _n8nServiceMock = new Mock<IN8nService>();
        }

        [Fact]
        public async Task Handle_ShouldCallUpsertAsync_ForEachFaceWithEmbedding()
        {
            // Arrange
            var command = new SaveFaceLabelsCommand
            {
                ImageId = Guid.NewGuid(),
                FaceLabels = new List<DetectedFaceDto>
                {
                    new DetectedFaceDto
                    {
                        Id = "face1",
                        MemberId = Guid.NewGuid(),
                        Embedding = new List<double> { 1.0, 2.0, 3.0 },
                        BoundingBox = new BoundingBoxDto()
                    },
                    new DetectedFaceDto
                    {
                        Id = "face2",
                        MemberId = Guid.NewGuid(),
                        Embedding = new List<double> { 4.0, 5.0, 6.0 },
                        BoundingBox = new BoundingBoxDto()
                    },
                    new DetectedFaceDto
                    {
                        Id = "face3",
                        MemberId = Guid.NewGuid(),
                        Embedding = null, // No embedding
                        BoundingBox = new BoundingBoxDto()
                    }
                }
            };

            _n8nServiceMock.Setup(x => x.CallEmbeddingWebhookAsync(It.IsAny<EmbeddingWebhookDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<string>.Success(string.Empty));

            var handler = new SaveFaceLabelsCommandHandler(
                _context,
                _configProviderMock.Object,
                _loggerMock.Object,
                _n8nServiceMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _n8nServiceMock.Verify(x => x.CallEmbeddingWebhookAsync(It.IsAny<EmbeddingWebhookDto>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }
}
