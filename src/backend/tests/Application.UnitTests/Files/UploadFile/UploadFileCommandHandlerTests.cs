using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Files.UploadFile;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Files.UploadFile;

public class UploadFileCommandHandlerTests : TestBase
{
    private readonly Mock<IFileStorage> _fileStorageMock;
    private readonly Mock<IConfigProvider> _configProviderMock;
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly UploadFileCommandHandler _handler;

    public UploadFileCommandHandlerTests()
    {
        _fileStorageMock = new Mock<IFileStorage>();
        _configProviderMock = new Mock<IConfigProvider>();
        _dateTimeMock = new Mock<IDateTime>();

        _configProviderMock.Setup(x => x.GetSection<StorageSettings>()).Returns(new StorageSettings
        {
            MaxFileSizeMB = 5, // 5 MB
            Provider = "Local"
        });

        _handler = new UploadFileCommandHandler(
            _fileStorageMock.Object,
            _configProviderMock.Object,
            _context,
            _dateTimeMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileIsEmpty()
    {
        // Arrange
        var command = new UploadFileCommand
        {
            FileStream = new MemoryStream(),
            FileName = "test.txt",
            ContentType = "text/plain",
            Length = 0
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.FileEmpty);
        result.ErrorSource.Should().Be(ErrorSources.Validation);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileSizeExceedsLimit()
    {
        // Arrange
        var command = new UploadFileCommand
        {
            FileStream = new MemoryStream(new byte[5 * 1024 * 1024 + 1]), // 5MB + 1 byte
            FileName = "large.jpg",
            ContentType = "image/jpeg",
            Length = 5 * 1024 * 1024 + 1
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.FileSizeExceedsLimit, 5));
        result.ErrorSource.Should().Be(ErrorSources.Validation);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileTypeIsInvalid()
    {
        // Arrange
        var command = new UploadFileCommand
        {
            FileStream = new MemoryStream(new byte[10]),
            FileName = "document.exe",
            ContentType = "application/octet-stream",
            Length = 10
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.InvalidFileType);
        result.ErrorSource.Should().Be(ErrorSources.Validation);
    }

    [Fact]
    public async Task Handle_ShouldUploadFileAndSaveMetadata_WhenValidCommand()
    {
        // Arrange
        var fileContent = new byte[] { 0x01, 0x02, 0x03 };
        var fileStream = new MemoryStream(fileContent);
        var fileName = "valid.png";
        var contentType = "image/png";
        var fileUrl = "http://example.com/uploaded/unique_valid.png";
        var now = DateTime.UtcNow;

        var command = new UploadFileCommand
        {
            FileStream = fileStream,
            FileName = fileName,
            ContentType = contentType,
            Length = fileContent.Length
        };

        _fileStorageMock.Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(fileUrl));
        _dateTimeMock.Setup(x => x.Now).Returns(now);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(fileUrl);

        _fileStorageMock.Verify(x => x.UploadFileAsync(It.IsAny<Stream>(), It.Is<string>(s => s.Contains("valid_") && s.EndsWith(".png")), contentType, CancellationToken.None), Times.Once);

        _context.FileMetadata.Should().ContainSingle(fm =>
            fm.FileName.Contains("valid_") &&
            fm.Url == fileUrl &&
            fm.ContentType == contentType &&
            fm.FileSize == fileContent.Length &&
            fm.StorageProvider == StorageProvider.Local &&
            fm.Created == now &&
            fm.LastModified == now);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileStorageUploadFails()
    {
        // Arrange
        var fileContent = new byte[] { 0x01, 0x02, 0x03 };
        var fileStream = new MemoryStream(fileContent);
        var fileName = "valid.png";
        var contentType = "image/png";

        var command = new UploadFileCommand
        {
            FileStream = fileStream,
            FileName = fileName,
            ContentType = contentType,
            Length = fileContent.Length
        };

        _fileStorageMock.Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure("Upload failed", ErrorSources.FileStorage));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Upload failed");
        result.ErrorSource.Should().Be(ErrorSources.FileStorage);
        _context.FileMetadata.Should().BeEmpty(); // No metadata should be saved
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileStorageReturnsNullUrl()
    {
        // Arrange
        var fileContent = new byte[] { 0x01, 0x02, 0x03 };
        var fileStream = new MemoryStream(fileContent);
        var fileName = "valid.png";
        var contentType = "image/png";

        var command = new UploadFileCommand
        {
            FileStream = fileStream,
            FileName = fileName,
            ContentType = contentType,
            Length = fileContent.Length
        };

        _fileStorageMock.Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(null!)); // Simulate null URL

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.FileUploadNullUrl);
        result.ErrorSource.Should().Be(ErrorSources.FileStorage);
        _context.FileMetadata.Should().BeEmpty(); // No metadata should be saved
    }

    [Fact]
    public async Task Handle_ShouldAddFileUsage_WhenEntityTypeAndIdAreProvided()
    {
        // Arrange
        var fileContent = new byte[] { 0x01, 0x02, 0x03 };
        var fileStream = new MemoryStream(fileContent);
        var fileName = "valid.png";
        var contentType = "image/png";
        var fileUrl = "http://example.com/uploaded/unique_valid.png";
        var entityType = "Family";
        var entityId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var command = new UploadFileCommand
        {
            FileStream = fileStream,
            FileName = fileName,
            ContentType = contentType,
            Length = fileContent.Length,
            EntityType = entityType,
            EntityId = entityId
        };

        _fileStorageMock.Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(fileUrl));
        _dateTimeMock.Setup(x => x.Now).Returns(now);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _context.FileMetadata.Should().ContainSingle();
        var fileMetadata = _context.FileMetadata.First();
        fileMetadata.FileUsages.Should().ContainSingle(fu =>
            fu.EntityType == entityType &&
            fu.EntityId == entityId &&
            fu.FileMetadataId == fileMetadata.Id);
    }

    [Theory]
    [InlineData("malicious/../../path/file.jpg", "file.jpg")]
    [InlineData("file with spaces.png", "filewithspaces.png")]
    [InlineData("file_with-dashes.pdf", "file_with-dashes.pdf")]
    [InlineData("file!@#$%^&*().docx", "file.docx")]
    public async Task Handle_ShouldSanitizeFileName(string inputFileName, string expectedSanitizedPart)
    {
        // Arrange
        var fileContent = new byte[] { 0x01, 0x02, 0x03 };
        var fileStream = new MemoryStream(fileContent);
        var contentType = "image/jpeg";
        var fileUrl = "http://example.com/uploaded/unique_file.jpg";

        var command = new UploadFileCommand
        {
            FileStream = fileStream,
            FileName = inputFileName,
            ContentType = contentType,
            Length = fileContent.Length
        };

        _fileStorageMock.Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(fileUrl));
        _dateTimeMock.Setup(x => x.Now).Returns(DateTime.UtcNow);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _fileStorageMock.Verify(x => x.UploadFileAsync(
            It.IsAny<Stream>(),
            It.Is<string>(s => s.Contains(Path.GetFileNameWithoutExtension(expectedSanitizedPart)) && s.EndsWith(Path.GetExtension(expectedSanitizedPart))),
            contentType,
            CancellationToken.None), Times.Once);
    }
}
