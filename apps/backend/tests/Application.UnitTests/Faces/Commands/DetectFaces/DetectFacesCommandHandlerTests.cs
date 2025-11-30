using backend.Application.AI.DTOs; // NEW
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Faces.Commands; // Added this using directive
using backend.Application.Faces.Commands.DetectFaces;
using backend.Application.Faces.Common;
using backend.Application.Faces.Queries;
using backend.Application.Faces.Queries.SearchMemberFace; // Add this line
using backend.Application.Files.UploadFile; // NEW
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using MediatR; // NEW
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Faces.Commands.DetectFaces
{
    public class DetectFacesCommandHandlerTests : TestBase
    {
        private readonly Mock<IFaceApiService> _faceApiServiceMock;

        private readonly Mock<ILogger<DetectFacesCommandHandler>> _loggerMock;
        private readonly Mock<IMediator> _mediatorMock;

        public DetectFacesCommandHandlerTests()
        {
            _faceApiServiceMock = new Mock<IFaceApiService>();

            _loggerMock = new Mock<ILogger<DetectFacesCommandHandler>>();
            _mediatorMock = new Mock<IMediator>();

            // Setup for mediator to handle UploadFileCommand
            _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<ImageUploadResponseDto>.Success(new ImageUploadResponseDto
                {
                    Url = "http://mock.uploaded.url/image.jpg",
                    Filename = "mock.jpg",
                    ContentType = "image/jpeg"
                }));

            // Default mock for IMediator.Send for SearchMemberFaceQuery
            var emptyFoundFacesResult = Result<List<FoundFaceDto>>.Success(new List<FoundFaceDto>());
            _mediatorMock.Setup(m => m.Send(It.IsAny<SearchMemberFaceQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(emptyFoundFacesResult));

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

            var memberId = Guid.NewGuid(); // Move declaration here
            var foundFacesResult = Result<List<FoundFaceDto>>.Success(new List<FoundFaceDto>
            {
                new FoundFaceDto { MemberId = memberId, Score = 0.1f }
            });

            // Setup the mediator to return the found member for SearchMemberFaceQuery
            _mediatorMock.Setup(m => m.Send(
                It.Is<SearchMemberFaceQuery>(q => q.Vector != null && q.Vector.SequenceEqual(detectedFaces.First().Embedding!)),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(foundFacesResult));

            var familyId = Guid.NewGuid();
            var family = new Family { Name = "Test Family", Code = "TF" };
            var member = new Member(memberId, "Test", "Member", "TM", familyId, family); // Fixed: Using new constructor
            family.Id = familyId; // Ensure family ID matches the one used for the member
            _context.Families.Add(family); // Add family to context
            _context.Members.Add(member);
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new DetectFacesCommandHandler(
                _faceApiServiceMock.Object,
                _context,
                _loggerMock.Object,
                _mediatorMock.Object);

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
                detectedFace.MemberName.Should().Be(member.FullName);
                detectedFace.FamilyId.Should().Be(familyId);
                detectedFace.FamilyName.Should().Be(family.Name);
                detectedFace.BirthYear.Should().Be(member.DateOfBirth?.Year);
                detectedFace.DeathYear.Should().Be(member.DateOfDeath?.Year);
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
                _loggerMock.Object,
                _mediatorMock.Object);

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
                _loggerMock.Object,
                _mediatorMock.Object);

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
                _loggerMock.Object,
                _mediatorMock.Object);

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
