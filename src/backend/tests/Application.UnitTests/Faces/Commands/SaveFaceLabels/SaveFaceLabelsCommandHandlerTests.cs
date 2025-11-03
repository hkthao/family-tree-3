using backend.Application.Common.Interfaces;
using backend.Application.Faces.Commands.SaveFaceLabels;
using backend.Application.UnitTests.Common;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Models.AppSetting;
using Microsoft.Extensions.Logging;
using backend.Application.Faces.Queries;
using backend.Application.Faces.Common;

namespace backend.Application.UnitTests.Faces.Commands.SaveFaceLabels
{
    public class SaveFaceLabelsCommandHandlerTests : TestBase
    {
        private readonly Mock<IVectorStoreFactory> _vectorStoreFactoryMock;
        private readonly Mock<IVectorStore> _vectorStoreMock;
        private readonly Mock<IConfigProvider> _configProviderMock;
        private readonly Mock<ILogger<SaveFaceLabelsCommandHandler>> _loggerMock;

        public SaveFaceLabelsCommandHandlerTests()
        {
            _vectorStoreFactoryMock = new Mock<IVectorStoreFactory>();
            _vectorStoreMock = new Mock<IVectorStore>();
            _configProviderMock = new Mock<IConfigProvider>();
            _loggerMock = new Mock<ILogger<SaveFaceLabelsCommandHandler>>();

            _vectorStoreFactoryMock.Setup(x => x.CreateVectorStore(It.IsAny<Domain.Enums.VectorStoreProviderType>()))
                .Returns(_vectorStoreMock.Object);

            _configProviderMock.Setup(x => x.GetSection<VectorStoreSettings>())
                .Returns(new VectorStoreSettings { Provider = "InMemory" });
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

            var handler = new SaveFaceLabelsCommandHandler(
                _loggerMock.Object,
                _vectorStoreFactoryMock.Object,
                _configProviderMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _vectorStoreMock.Verify(x => x.UpsertAsync(It.IsAny<List<double>>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Exactly(2)); // Fixed: Changed double[] to List<double>
        }
    }
}