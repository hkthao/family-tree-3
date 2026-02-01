using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

using backend.Application.MemberFaces.Commands.DetectFaces;
using backend.Application.MemberFaces.Common;
using backend.Application.MemberFaces.Queries.SearchVectorFace;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using MediatR; // Re-add MediatR
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



            // Default mock for IAuthorizationService
            _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
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
                    BoundingBox = new BoundingBoxDto { X = 10, Y = 10, Width = 50, Height = 50 },
                    Confidence = 0.99f,
                    Embedding = new List<float> { 1.0f, 2.0f, 3.0f }
                }
            };

            _faceApiServiceMock.Setup(x => x.DetectFacesAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(detectedFaces);

            var memberId = Guid.NewGuid();
            _mockUser.Setup(x => x.UserId).Returns(memberId); // Set the current user to the member being added
            var familyId = Guid.NewGuid();
            var family = new Family { Id = familyId, Name = "Test Family", Code = "TF" };
            _context.Families.Add(family); // Add family to context

            var member = new Member("Test", "Member", "TM", familyId);
            member.SetId(memberId); // Explicitly set the ID
            _context.Members.Add(member);
            await _context.SaveChangesAsync(CancellationToken.None);

            // Setup the _faceApiServiceMock to return the found member for BatchSearchSimilarFacesAsync
            _faceApiServiceMock.Setup(x => x.BatchSearchSimilarFacesAsync(It.IsAny<BatchFaceSearchVectorRequestDto>()))
                .ReturnsAsync(new List<List<FaceApiSearchResultDto>>
                {
                    new List<FaceApiSearchResultDto>
                    {
                        new FaceApiSearchResultDto
                        {
                            Id = Guid.NewGuid().ToString(),
                            Score = 0.9f,
                            Payload = new FaceSearchResultPayloadDto
                            {
                                FaceId = Guid.NewGuid(),
                                MemberId = memberId,
                                FamilyId = familyId,
                                BoundingBox = new BoundingBoxDto { X = 10, Y = 10, Width = 50, Height = 50 },
                                Confidence = 0.88f
                            }
                        }
                    }
                });

            var handler = new DetectFacesCommandHandler(
                _faceApiServiceMock.Object,
                _context,
                _loggerMock.Object,
                _mediatorMock.Object, // Re-add IMediator mock
                _mockUser.Object, // Pass ICurrentUser mock
                _mockAuthorizationService.Object); // Pass IAuthorizationService mock

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            if (!result.IsSuccess)
            {
                result.Error.Should().NotBeNullOrEmpty();
                _loggerMock.Object.Log(LogLevel.Error, $"Test failed with error: {result.Error}");
            }
            result.IsSuccess.Should().BeTrue(because: result.Error);
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
                _mediatorMock.Object, // Re-add IMediator mock
                _mockUser.Object, // Pass ICurrentUser mock
                _mockAuthorizationService.Object); // Pass IAuthorizationService mock

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
                _mediatorMock.Object, // Re-add IMediator mock
                _mockUser.Object, // Pass ICurrentUser mock
                _mockAuthorizationService.Object); // Pass IAuthorizationService mock

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
                        BoundingBox = new BoundingBoxDto { X = 10, Y = 10, Width = 50, Height = 50 },
                        Confidence = 0.99f,
                        Thumbnail = null // Should be null when ReturnCrop is false
                    }
                });

            _faceApiServiceMock.Setup(x => x.BatchSearchSimilarFacesAsync(It.IsAny<BatchFaceSearchVectorRequestDto>()))
                .ReturnsAsync(new List<List<FaceApiSearchResultDto>>());

            _faceApiServiceMock.Setup(x => x.BatchSearchSimilarFacesAsync(It.IsAny<BatchFaceSearchVectorRequestDto>()))
                .ReturnsAsync(new List<List<FaceApiSearchResultDto>>());



                        var handler = new DetectFacesCommandHandler(



                            _faceApiServiceMock.Object,



                            _context,



                            _loggerMock.Object,

                            _mediatorMock.Object, // Re-add IMediator mock

                            _mockUser.Object, // Pass ICurrentUser mock



                            _mockAuthorizationService.Object); // Pass IAuthorizationService mock

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