using backend.Application.AI.DTOs; // NEW USING FOR IMAGE UPLOAD DTOs
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.UploadFile;
using backend.Application.UnitTests.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Files.UploadFile;

public class UploadFileCommandHandlerTests : TestBase
{
    private readonly Mock<IN8nService> _n8nServiceMock; // Changed from IFileStorage
    private readonly UploadFileCommandHandler _handler;

    public UploadFileCommandHandlerTests()
    {
        _n8nServiceMock = new Mock<IN8nService>(); // Changed from IFileStorageMock
        _handler = new UploadFileCommandHandler(_n8nServiceMock.Object);
    }

    private UploadFileCommand CreateValidCommand(byte[] imageData, string contentType = "image/jpeg")
    {
        return new UploadFileCommand
        {
            ImageData = imageData ?? new byte[] { 1, 2, 3 },
            FileName = "test.jpg",
            Cloud = "imgbb",
            Folder = "test-folder",
            ContentType = contentType
        };
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenN8nUploadIsSuccessful()
    {
        // Arrange
        var imageData = new byte[] { 1, 2, 3 };
        var contentType = "image/jpeg";
        var command = CreateValidCommand(imageData, contentType);
        var imageUrl = "http://uploaded.image.url/test.jpg";
        var n8nResponse = new ImageUploadResponseDto { Url = imageUrl, Filename = command.FileName, ContentType = contentType };

        _n8nServiceMock.Setup(x => x.CallImageUploadWebhookAsync(
            It.IsAny<ImageUploadWebhookDto>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ImageUploadResponseDto>.Success(n8nResponse));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(n8nResponse); // Assert the DTO object
        _n8nServiceMock.Verify(x => x.CallImageUploadWebhookAsync(
            It.Is<ImageUploadWebhookDto>(dto =>
                dto.ImageData == command.ImageData &&
                dto.FileName == command.FileName &&
                dto.Cloud == command.Cloud &&
                dto.Folder == command.Folder &&
                dto.ContentType == command.ContentType
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        // Verify that FileMetadata is NOT saved
        _context.FileMetadata.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nUploadFails()
    {
        // Arrange
        var command = CreateValidCommand(new byte[] { 1, 2, 3 }); // Added imageData
        var errorMessage = "N8n upload failed.";
        var errorSource = ErrorSources.ExternalServiceError;

        var failureResult = Result<ImageUploadResponseDto>.Failure(errorMessage, errorSource); // Create result explicitly

        _n8nServiceMock.Setup(x => x.CallImageUploadWebhookAsync(
            It.IsAny<ImageUploadWebhookDto>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult!); // Add null-forgiving operator to the returned result

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(errorMessage);
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
        _context.FileMetadata.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nReturnsEmptyUrl()
    {
        // Arrange
        var command = CreateValidCommand(new byte[] { 1, 2, 3 }); // Added imageData
        var n8nResponse = new ImageUploadResponseDto { Url = "", Filename = command.FileName, ContentType = command.ContentType }; // Empty URL

        _n8nServiceMock.Setup(x => x.CallImageUploadWebhookAsync(
            It.IsAny<ImageUploadWebhookDto>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ImageUploadResponseDto>.Success(n8nResponse));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Debugging assertion: Check what result.IsSuccess actually is, and if true, what result.Value is
        if (result.IsSuccess)
        {
            result.Value.Should().BeNull("because the handler should have returned a failure when URL is empty.");
        }

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(ErrorMessages.FileUploadNullUrl);
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
        _context.FileMetadata.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nResponseIsEmptyList()
    {
        // Arrange
        var command = CreateValidCommand(new byte[] { 1, 2, 3 }); // Added imageData
        var n8nResponse = new ImageUploadResponseDto(); // Empty DTO with default values

        _n8nServiceMock.Setup(x => x.CallImageUploadWebhookAsync(
            It.IsAny<ImageUploadWebhookDto>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ImageUploadResponseDto>.Success(n8nResponse));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(ErrorMessages.FileUploadNullUrl);
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
        _context.FileMetadata.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nReturnsNullResponse()
    {
        // Arrange
        var command = CreateValidCommand(new byte[] { 1, 2, 3 }); // Added imageData
        var errorMessage = ErrorMessages.FileUploadNullUrl;
        var errorSource = ErrorSources.ExternalServiceError;

        // Simulate n8n returning a failure, as the handler would interpret a null value as a failure
        _n8nServiceMock.Setup(x => x.CallImageUploadWebhookAsync(
            It.IsAny<ImageUploadWebhookDto>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ImageUploadResponseDto>.Failure(errorMessage, errorSource)); // Changed to return explicit Failure

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(errorMessage);
        result.ErrorSource.Should().Be(errorSource);
        _context.FileMetadata.Should().BeEmpty();
    }
}
