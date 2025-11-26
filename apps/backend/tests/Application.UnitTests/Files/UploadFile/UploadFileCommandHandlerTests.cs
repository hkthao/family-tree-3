using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Files.UploadFile;
using backend.Application.UnitTests.Common;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Moq;
using System.Net;
using System.Text.Json;
using System.Threading;
using Xunit;
using backend.Application.AI.DTOs; // NEW USING FOR IMAGE UPLOAD DTOs

namespace backend.Application.UnitTests.Files.UploadFile;

public class UploadFileCommandHandlerTests : TestBase
{
    private readonly Mock<IN8nService> _n8nServiceMock; // Changed from IFileStorage
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly IConfiguration _configuration;
    private readonly UploadFileCommandHandler _handler;

    public UploadFileCommandHandlerTests()
    {
        _n8nServiceMock = new Mock<IN8nService>(); // Changed from IFileStorageMock
        _dateTimeMock = new Mock<IDateTime>();

        var inMemorySettings = new Dictionary<string, string?> {
            {$"{nameof(StorageSettings)}:MaxFileSizeMB", "5"},
            {$"{nameof(StorageSettings)}:Provider", "Local"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _handler = new UploadFileCommandHandler(
            _n8nServiceMock.Object, // Changed from _fileStorageMock.Object
            _configuration,
            _context,
            _dateTimeMock.Object);
    }

    private UploadFileCommand CreateValidCommand(byte[] imageData)
    {
        return new UploadFileCommand
        {
            ImageData = imageData ?? new byte[] { 1, 2, 3 },
            FileName = "test.jpg",
            Cloud = "imgbb",
            Folder = "test-folder"
        };
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenN8nUploadIsSuccessful()
    {
        // Arrange
        var command = CreateValidCommand(new byte[] { 1, 2, 3 }); // Added imageData
        var imageUrl = "http://uploaded.image.url/test.jpg";
        var n8nResponse = new List<ImageUploadResponseDto> { new ImageUploadResponseDto { Url = imageUrl } };

        _n8nServiceMock.Setup(x => x.CallImageUploadWebhookAsync(
            It.IsAny<ImageUploadWebhookDto>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<ImageUploadResponseDto>>.Success(n8nResponse));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(imageUrl);
        _n8nServiceMock.Verify(x => x.CallImageUploadWebhookAsync(
            It.Is<ImageUploadWebhookDto>(dto =>
                dto.ImageData == command.ImageData &&
                dto.FileName == command.FileName &&
                dto.Cloud == command.Cloud &&
                dto.Folder == command.Folder
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

        var failureResult = Result<List<ImageUploadResponseDto>>.Failure(errorMessage, errorSource); // Create result explicitly

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
        var n8nResponse = new List<ImageUploadResponseDto> { new ImageUploadResponseDto { Url = "" } }; // Empty URL

        _n8nServiceMock.Setup(x => x.CallImageUploadWebhookAsync(
            It.IsAny<ImageUploadWebhookDto>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<ImageUploadResponseDto>>.Success(n8nResponse));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

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
        var n8nResponse = new List<ImageUploadResponseDto>(); // Empty list

        _n8nServiceMock.Setup(x => x.CallImageUploadWebhookAsync(
            It.IsAny<ImageUploadWebhookDto>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<ImageUploadResponseDto>>.Success(n8nResponse));

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
        
        // Simulate n8n returning a list where the first item has an empty URL
        _n8nServiceMock.Setup(x => x.CallImageUploadWebhookAsync(
            It.IsAny<ImageUploadWebhookDto>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<ImageUploadResponseDto>>.Success(new List<ImageUploadResponseDto> { new ImageUploadResponseDto { Url = string.Empty, DisplayUrl = string.Empty, Width = 0, Height = 0, Size = 0, Id = string.Empty, Name = string.Empty, Filename = string.Empty, Extension = string.Empty } })); // Simulate empty URL

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);


        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(ErrorMessages.FileUploadNullUrl);
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
        _context.FileMetadata.Should().BeEmpty();
    }
}
