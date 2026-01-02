using backend.Application.ImageRestorationJobs.Commands.CreateImageRestorationJob;
using backend.Application.ImageRestorationJobs.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Application.Common.Interfaces;
using backend.Application.UnitTests.Common;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using backend.Application.Common.Models.ImageRestoration;
using backend.Application.Common.Models;
using MediatR; // Added
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia; // Added
using backend.Application.FamilyMedias.DTOs; // Added

namespace backend.Application.UnitTests.ImageRestorationJobs;

public class CreateImageRestorationJobCommandTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<CreateImageRestorationJobCommandHandler>> _loggerMock;
    private readonly Mock<IImageRestorationService> _imageRestorationServiceMock;
    private readonly Mock<IMediator> _mediatorMock; // Added
    private readonly DateTime _fixedDateTime;

    public CreateImageRestorationJobCommandTests()
    {
        _currentUserMock = _mockUser;
        _loggerMock = new Mock<ILogger<CreateImageRestorationJobCommandHandler>>();
        _imageRestorationServiceMock = new Mock<IImageRestorationService>();
        _mediatorMock = new Mock<IMediator>(); // Initialized

        _fixedDateTime = DateTime.Now;
        _mockDateTime.Setup(d => d.Now).Returns(_fixedDateTime);

        _currentUserMock.Setup(s => s.IsAuthenticated).Returns(true);

        // Setup for mediator to handle CreateFamilyMediaCommand
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FamilyMediaDto>.Success(new FamilyMediaDto { FilePath = "http://uploaded.image/url" })); // Mock successful upload
    }

    [Fact]
    public async Task Handle_GivenValidRequest_ShouldCreateImageRestorationJob()
    {
        // Arrange
        var command = new CreateImageRestorationJobCommand(
            ImageData: new byte[] { 0x01, 0x02, 0x03 },
            FileName: "test.jpg",
            ContentType: "image/jpeg",
            FamilyId: Guid.NewGuid()
        );

        var successfulRestorationResponse = new StartImageRestorationResponseDto
        {
            JobId = Guid.NewGuid(),
            Status = RestorationStatus.Processing,
            OriginalUrl = "http://uploaded.image/url",
            RestoredUrl = "http://restored.image/url" // Added for complete flow
        };
        var successfulRestorationResult = Result<StartImageRestorationResponseDto>.Success(successfulRestorationResponse);

        _imageRestorationServiceMock.Setup(s => s.PreprocessImageAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PreprocessImageResponseDto>.Success(new PreprocessImageResponseDto
            {
                ProcessedImageBase64 = "data:image/jpeg;base64,AQID", // Base64 of 0x01,0x02,0x03
                IsResized = false
            }));

        _imageRestorationServiceMock.Setup(s => s.StartRestorationAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successfulRestorationResult);

        var handler = new CreateImageRestorationJobCommandHandler(
            _context,
            _mapper,
            _currentUserMock.Object,
            _mockDateTime.Object,
            _loggerMock.Object,
            _imageRestorationServiceMock.Object,
            _mediatorMock.Object // Pass mediator mock
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.IsType<ImageRestorationJobDto>(result.Value);

        var createdJob = _context.ImageRestorationJobs.FirstOrDefault(j => j.OriginalImageUrl == "http://uploaded.image/url");
        Assert.NotNull(createdJob);
        Assert.Equal("http://uploaded.image/url", createdJob.OriginalImageUrl);
        Assert.Equal(command.FamilyId, createdJob.FamilyId);
        Assert.Equal(RestorationStatus.Completed, createdJob.Status); // Should be completed
        Assert.Equal("http://restored.image/url", createdJob.RestoredImageUrl); // Check restored URL
        Assert.Equal(_currentUserMock.Object.UserId.ToString(), createdJob.CreatedBy);
        Assert.Equal(_fixedDateTime, createdJob.Created);

        // Verify that preprocessing and restoration services were called
        _imageRestorationServiceMock.Verify(s => s.PreprocessImageAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _imageRestorationServiceMock.Verify(s => s.StartRestorationAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenUnauthenticatedUser_ShouldReturnFailure()
    {
        // Arrange
        _currentUserMock.Setup(s => s.UserId).Returns(Guid.Empty);
        _currentUserMock.Setup(s => s.IsAuthenticated).Returns(false);

        var command = new CreateImageRestorationJobCommand(
            ImageData: new byte[] { 0x01 },
            FileName: "test.jpg",
            ContentType: "image/jpeg",
            FamilyId: Guid.NewGuid()
        );

        var handler = new CreateImageRestorationJobCommandHandler(
            _context,
            _mapper,
            _currentUserMock.Object,
            _mockDateTime.Object,
            _loggerMock.Object,
            _imageRestorationServiceMock.Object,
            _mediatorMock.Object // Pass mediator mock
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User is not authenticated.", result.Error);
    }

    [Fact]
    public async Task Handle_GivenEmptyImageData_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateImageRestorationJobCommand(
            ImageData: Array.Empty<byte>(),
            FileName: "test.jpg",
            ContentType: "image/jpeg",
            FamilyId: Guid.NewGuid()
        );

        var validator = new CreateImageRestorationJobCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Contains("Image data is required.", validationResult.Errors.Select(e => e.ErrorMessage));
    }

    [Fact]
    public async Task Handle_GivenEmptyFileName_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateImageRestorationJobCommand(
            ImageData: new byte[] { 0x01 },
            FileName: "",
            ContentType: "image/jpeg",
            FamilyId: Guid.NewGuid()
        );

        var validator = new CreateImageRestorationJobCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Contains("File name is required.", validationResult.Errors.Select(e => e.ErrorMessage));
    }

    [Fact]
    public async Task Handle_GivenInvalidContentType_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateImageRestorationJobCommand(
            ImageData: new byte[] { 0x01 },
            FileName: "test.txt",
            ContentType: "text/plain",
            FamilyId: Guid.NewGuid()
        );

        var validator = new CreateImageRestorationJobCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Contains("Content type must be an image type (e.g., image/jpeg).", validationResult.Errors.Select(e => e.ErrorMessage));
    }

    [Fact]
    public async Task Handle_GivenEmptyFamilyId_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateImageRestorationJobCommand(
            ImageData: new byte[] { 0x01 },
            FileName: "test.jpg",
            ContentType: "image/jpeg",
            FamilyId: Guid.Empty
        );

        var validator = new CreateImageRestorationJobCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Contains("Family ID is required.", validationResult.Errors.Select(e => e.ErrorMessage));
    }

    [Fact]
    public async Task Handle_GivenPreprocessingFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateImageRestorationJobCommand(
            ImageData: new byte[] { 0x01 },
            FileName: "test.jpg",
            ContentType: "image/jpeg",
            FamilyId: Guid.NewGuid()
        );

        _imageRestorationServiceMock.Setup(s => s.PreprocessImageAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PreprocessImageResponseDto>.Failure("Preprocessing failed."));

        var handler = new CreateImageRestorationJobCommandHandler(
            _context,
            _mapper,
            _currentUserMock.Object,
            _mockDateTime.Object,
            _loggerMock.Object,
            _imageRestorationServiceMock.Object,
            _mediatorMock.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Image preprocessing failed", result.Error);
    }

    [Fact]
    public async Task Handle_GivenImageUploadFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateImageRestorationJobCommand(
            ImageData: new byte[] { 0x01 },
            FileName: "test.jpg",
            ContentType: "image/jpeg",
            FamilyId: Guid.NewGuid()
        );

        _imageRestorationServiceMock.Setup(s => s.PreprocessImageAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PreprocessImageResponseDto>.Success(new PreprocessImageResponseDto
            {
                ProcessedImageBase64 = "data:image/jpeg;base64,AQID",
                IsResized = false
            }));

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FamilyMediaDto>.Failure("Upload failed."));

        var handler = new CreateImageRestorationJobCommandHandler(
            _context,
            _mapper,
            _currentUserMock.Object,
            _mockDateTime.Object,
            _loggerMock.Object,
            _imageRestorationServiceMock.Object,
            _mediatorMock.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to upload image", result.Error);
    }

    [Fact]
    public async Task Handle_GivenRestorationServiceFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateImageRestorationJobCommand(
            ImageData: new byte[] { 0x01 },
            FileName: "test.jpg",
            ContentType: "image/jpeg",
            FamilyId: Guid.NewGuid()
        );

        _imageRestorationServiceMock.Setup(s => s.PreprocessImageAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PreprocessImageResponseDto>.Success(new PreprocessImageResponseDto
            {
                ProcessedImageBase64 = "data:image/jpeg;base64,AQID",
                IsResized = false
            }));

        _imageRestorationServiceMock.Setup(s => s.StartRestorationAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<StartImageRestorationResponseDto>.Failure("Restoration service failed."));

        var handler = new CreateImageRestorationJobCommandHandler(
            _context,
            _mapper,
            _currentUserMock.Object,
            _mockDateTime.Object,
            _loggerMock.Object,
            _imageRestorationServiceMock.Object,
            _mediatorMock.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to start image restoration", result.Error);

        // Verify job status is marked as failed
        var createdJob = _context.ImageRestorationJobs.FirstOrDefault();
        Assert.NotNull(createdJob);
        Assert.Equal(RestorationStatus.Failed, createdJob.Status);
    }
}
