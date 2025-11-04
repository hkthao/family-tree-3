using backend.Application.AI.VectorStore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Faces.Commands; // Added this using directive
using backend.Application.Faces.Commands.DetectFaces;
using backend.Application.Faces.Common;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Faces.Commands.DetectFaces
{
    public class DetectFacesCommandHandlerTests : TestBase
    {
        private readonly Mock<IFaceApiService> _faceApiServiceMock;
        private readonly Mock<IVectorStoreFactory> _vectorStoreFactoryMock;
        private readonly Mock<IVectorStore> _vectorStoreMock;
        private readonly Mock<IConfigProvider> _configProviderMock;
        private readonly Mock<ILogger<DetectFacesCommandHandler>> _loggerMock;

        public DetectFacesCommandHandlerTests()
        {
            _faceApiServiceMock = new Mock<IFaceApiService>();
            _vectorStoreFactoryMock = new Mock<IVectorStoreFactory>();
            _vectorStoreMock = new Mock<IVectorStore>();
            _configProviderMock = new Mock<IConfigProvider>();
            _loggerMock = new Mock<ILogger<DetectFacesCommandHandler>>();

            _vectorStoreFactoryMock.Setup(x => x.CreateVectorStore(It.IsAny<Domain.Enums.VectorStoreProviderType>()))
                .Returns(_vectorStoreMock.Object);

            _configProviderMock.Setup(x => x.GetSection<VectorStoreSettings>())
                .Returns(new VectorStoreSettings { Provider = "InMemory" });
        }

        [Fact]
        public async Task Handle_ShouldReturnDetectedFaces_WhenFacesAreDetected()
        {
            // Arrange
            var command = new DetectFacesCommand
            {
                ImageBytes = new byte[0],
                ContentType = "image/jpeg"
            };

            var detectedFaces = new List<FaceDetectionResultDto>
            {
                new FaceDetectionResultDto
                {
                    Id = "face1",
                    BoundingBox = new BoundingBoxDto { X = 10, Y = 10, Width = 50, Height = 50 },
                    Confidence = 0.99f,
                    Embedding = new double[] { 1.0, 2.0, 3.0 }
                }
            };

            _faceApiServiceMock.Setup(x => x.DetectFacesAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(detectedFaces);

            var memberId = Guid.NewGuid();
            var familyId = Guid.NewGuid();
            var family = new Family { Name = "Test Family", Code = "TF" };
            var member = new Member(memberId, "Test", "Member", "TM", familyId, family); // Fixed: Using new constructor
            _context.Members.Add(member);
            await _context.SaveChangesAsync(CancellationToken.None);

            var queryResult = new List<VectorStoreQueryResult> // Fixed: Using VectorStoreQueryResult
            {
                new VectorStoreQueryResult
                {
                    Score = 0.9f,
                    Metadata = new Dictionary<string, string>
                    {
                        { "member_id", memberId.ToString() }
                    }
                }
            };

            _vectorStoreMock.Setup(x => x.QueryAsync(It.IsAny<double[]>(), It.IsAny<int>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);

            var handler = new DetectFacesCommandHandler(
                _faceApiServiceMock.Object,
                _context,
                _vectorStoreFactoryMock.Object,
                _configProviderMock.Object,
                _loggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull(); // Added null check
            if (result.Value != null)
            {
                result.Value.DetectedFaces.Should().HaveCount(1);
                var detectedFace = result.Value.DetectedFaces[0];
                detectedFace.MemberId.Should().Be(memberId);
                detectedFace.MemberName.Should().Be("Member Test");
            }
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoFacesAreDetected()
        {
            // Arrange
            var command = new DetectFacesCommand
            {
                ImageBytes = new byte[0],
                ContentType = "image/jpeg"
            };

            _faceApiServiceMock.Setup(x => x.DetectFacesAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<FaceDetectionResultDto>());

            var handler = new DetectFacesCommandHandler(
                _faceApiServiceMock.Object,
                _context,
                _vectorStoreFactoryMock.Object,
                _configProviderMock.Object,
                _loggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.DetectedFaces.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenFaceDetectionServiceThrowsException()
        {
            // Arrange
            var command = new DetectFacesCommand
            {
                ImageBytes = new byte[0],
                ContentType = "image/jpeg"
            };

            _faceApiServiceMock.Setup(x => x.DetectFacesAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new System.Exception("Face detection service error"));

            var handler = new DetectFacesCommandHandler(
                _faceApiServiceMock.Object,
                _context,
                _vectorStoreFactoryMock.Object,
                _configProviderMock.Object,
                _loggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Face detection service error");
        }

        [Fact]
        public async Task Handle_ShouldReturnNullThumbnail_WhenReturnCropIsFalse()
        {
            // Arrange
            var command = new DetectFacesCommand
            {
                ImageBytes = new byte[0],
                ContentType = "image/jpeg",
                ReturnCrop = false
            };

            _faceApiServiceMock.Setup(x => x.DetectFacesAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.Is<bool>(b => b == false)))
                .ReturnsAsync(new List<FaceDetectionResultDto>
                {
                    new FaceDetectionResultDto
                    {
                        Id = "face1",
                        BoundingBox = new BoundingBoxDto { X = 10, Y = 10, Width = 50, Height = 50 },
                        Confidence = 0.99f,
                        Embedding = new double[] { 1.0, 2.0, 3.0 },
                        Thumbnail = null // Should be null when ReturnCrop is false
                    }
                });

            var handler = new DetectFacesCommandHandler(
                _faceApiServiceMock.Object,
                _context,
                _vectorStoreFactoryMock.Object,
                _configProviderMock.Object,
                _loggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            if (result.Value != null)
            {
                result.Value.DetectedFaces.Should().HaveCount(1);
                var detectedFace = result.Value.DetectedFaces[0];
                detectedFace.Thumbnail.Should().BeNull();
            }
        }
    }
}
