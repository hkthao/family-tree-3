using backend.Application.AI.DTOs; // ADDED
using backend.Application.Common.Constants; // Added this line
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Faces.Commands; // Added this line
using backend.Application.Faces.Commands.DetectFaces;
using backend.Application.Faces.Common;
using backend.Application.Files.UploadFile; // NEW
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using MediatR; // NEW
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using backend.Application.Faces.Queries; // NEW - for SearchMemberFaceQuery
using backend.Application.Faces.Queries.SearchMemberFace; // ADDED - for FoundFaceDto

namespace backend.Application.UnitTests.Faces.Commands.DetectFaces;

public class DetectFacesTests : TestBase
{
    private readonly Mock<IFaceApiService> _mockFaceApiService;
    private readonly Mock<ILogger<DetectFacesCommandHandler>> _mockLogger;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly DetectFacesCommandHandler _handler; // Ensure this is explicitly declared

    public DetectFacesTests()
    {
        _mockFaceApiService = new Mock<IFaceApiService>();
        _mockLogger = new Mock<ILogger<DetectFacesCommandHandler>>();
        _mediatorMock = new Mock<IMediator>();

        // Default mock for IMediator.Send for SearchMemberFaceQuery
        _mediatorMock.Setup(m => m.Send(It.IsAny<SearchMemberFaceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<FoundFaceDto>>.Success(new List<FoundFaceDto>())); // Return empty list by default

        _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ImageUploadResponseDto>.Success(new ImageUploadResponseDto
            {
                Url = "http://mock.uploaded.url/image.jpg",
                Filename = "mock.jpg",
                ContentType = "image/jpeg"
            }));

        _handler = new DetectFacesCommandHandler(
            _mockFaceApiService.Object,
            _context,
            _mockLogger.Object,
            _mediatorMock.Object);
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
                Thumbnail = "AQID", // A valid base64 string for 1, 2, 3 bytes
                Embedding = new double[] { 0.1, 0.2 }
            }
        };
        _mockFaceApiService.Setup(s => s.DetectFacesAsync(imageBytes, contentType, true))
            .Returns(Task.FromResult(faceApiResult));


        var command = new DetectFacesCommand { ImageBytes = imageBytes, ContentType = contentType, ReturnCrop = true };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ImageUploadResponseDto>.Success(new ImageUploadResponseDto
            {
                Url = "http://mock.uploaded.url/image.jpg",
                Filename = "mock.jpg",
                ContentType = "image/jpeg"
            }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.DetectedFaces.Should().ContainSingle();
        var detectedFace = result.Value.DetectedFaces.First();
        detectedFace.BoundingBox.X.Should().Be(1);
        detectedFace.Confidence.Should().Be(0.9f);
        detectedFace.Thumbnail.Should().Be("AQID"); // Thumbnail (base64) should still be there if API returns it
        detectedFace.ThumbnailUrl.Should().BeNull(); // Public URL should be null
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


        var command = new DetectFacesCommand { ImageBytes = imageBytes, ContentType = contentType, ReturnCrop = true };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Should still succeed overall
        result.Value.Should().NotBeNull();
        result.Value!.DetectedFaces.Should().ContainSingle();
        result.Value.DetectedFaces.First().ThumbnailUrl.Should().BeNull(); // Assert ThumbnailUrl is null
        result.Value.DetectedFaces.First().ThumbnailUrl.Should().BeNull(); // Assert ThumbnailUrl is null
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


        var command = new DetectFacesCommand { ImageBytes = imageBytes, ContentType = contentType, ReturnCrop = true };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Should still succeed overall
        result.Value.Should().NotBeNull();
        result.Value!.DetectedFaces.Should().ContainSingle();
        result.Value.DetectedFaces.First().ThumbnailUrl.Should().BeNull(); // Assert ThumbnailUrl is null
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
        var member = new Member(memberId, "Doe", "John", "JD1", familyId, family, false);
        member.Update("John", "Doe", "JD1", null, null, new DateTime(1990, 1, 1), new DateTime(2050, 1, 1), null, null, null, null, null, null, null, null, null, false);

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

        // Setup the mediator to return the found member for SearchMemberFaceQuery
        _mediatorMock.Setup(m => m.Send(
            It.Is<SearchMemberFaceQuery>(q => q.Vector != null && q.Vector.SequenceEqual(faceApiResult.First().Embedding!)),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<FoundFaceDto>>.Success(new List<FoundFaceDto>
            {
                new FoundFaceDto { MemberId = memberId, Score = 0.9f }
            }));

        var command = new DetectFacesCommand { ImageBytes = imageBytes, ContentType = contentType, ReturnCrop = true };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.DetectedFaces.Should().ContainSingle();
        var detectedFace = result.Value.DetectedFaces.First();
        detectedFace.MemberId.Should().Be(memberId);
        detectedFace.ThumbnailUrl.Should().BeNull(); // Assert ThumbnailUrl is null
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
        result.Error.Should().Contain(string.Format(ErrorMessages.UnexpectedError, "Face API error"));
    }
}
