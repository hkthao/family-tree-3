using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.DTOs;
using backend.Application.Files.UploadFile;
using backend.Application.UnitTests.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Files.UploadFile;

public class UploadFileCommandHandlerTests : TestBase
{
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly UploadFileCommandHandler _handler;

    public UploadFileCommandHandlerTests()
    {
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _handler = new UploadFileCommandHandler(_fileStorageServiceMock.Object);
    }

    private UploadFileCommand CreateValidCommand(byte[] imageData, string contentType = "image/jpeg")
    {
        return new UploadFileCommand
        {
            ImageData = imageData ?? new byte[] { 1, 2, 3 },
            FileName = "test.jpg",
            Folder = "test-folder",
            ContentType = contentType
        };
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenFileStorageUploadIsSuccessful()
    {
        // Arrange
        var imageData = new byte[] { 1, 2, 3 };
        var contentType = "image/jpeg";
        var command = CreateValidCommand(imageData, contentType);
        var imageUrl = "http://uploaded.image.url/test.jpg";
        var expectedResponse = new ImageUploadResponseDto { Url = imageUrl, DisplayUrl = imageUrl, Filename = command.FileName, ContentType = contentType, Extension = Path.GetExtension(command.FileName) };

        _fileStorageServiceMock.Setup(x => x.UploadFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(imageUrl)); // IFileStorageService returns Result<string>

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedResponse);
        _fileStorageServiceMock.Verify(x => x.UploadFileAsync(
            It.IsAny<Stream>(),
            command.FileName,
            command.Folder,
            It.IsAny<CancellationToken>()
        ), Times.Once);

        // Verify that FileMetadata is NOT saved (as per UploadFileCommandHandler logic)
        _context.FileMetadata.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileStorageUploadFails()
    {
        // Arrange
        var command = CreateValidCommand(new byte[] { 1, 2, 3 });
        var errorMessage = "File storage upload failed.";
        var errorSource = ErrorSources.ExternalServiceError;

        _fileStorageServiceMock.Setup(x => x.UploadFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure(errorMessage, errorSource));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(errorMessage);
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
        _context.FileMetadata.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileStorageReturnsEmptyUrl()
    {
        // Arrange
        var command = CreateValidCommand(new byte[] { 1, 2, 3 });
        var emptyUrl = "";

        _fileStorageServiceMock.Setup(x => x.UploadFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(emptyUrl));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(ErrorMessages.FileUploadNullUrl);
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
        _context.FileMetadata.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileStorageReturnsNullUrlResult()
    {
        // Arrange
        var command = CreateValidCommand(new byte[] { 1, 2, 3 });
        var errorMessage = "Null URL result from file storage.";

        _fileStorageServiceMock.Setup(x => x.UploadFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure(errorMessage, ErrorSources.ExternalServiceError)); // Simulate null/empty URL result as failure

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(errorMessage);
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
        _context.FileMetadata.Should().BeEmpty();
    }
}
