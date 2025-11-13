using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.DeleteFile;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Files.DeleteFile;

public class DeleteFileCommandHandlerTests : TestBase
{
    private readonly Mock<IFileStorage> _fileStorageMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly DeleteFileCommandHandler _handler;

    public DeleteFileCommandHandlerTests()
    {
        _fileStorageMock = new Mock<IFileStorage>();
        _currentUserMock = new Mock<ICurrentUser>();
        _dateTimeMock = new Mock<IDateTime>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();

        _handler = new DeleteFileCommandHandler(
            _context,
            _fileStorageMock.Object,
            _currentUserMock.Object,
            _dateTimeMock.Object,
            _authorizationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileMetadataNotFound()
    {
        // Arrange
        var command = new DeleteFileCommand { FileId = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, "File metadata"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAccessDenied()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var fileMetadata = new FileMetadata { Id = fileId, CreatedBy = otherUserId.ToString(), FileName = "test.txt", ContentType = "text/plain", Url = "http://example.com/test.txt" };
        await _context.FileMetadata.AddAsync(fileMetadata);
        await _context.SaveChangesAsync(CancellationToken.None);

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(false);

        var command = new DeleteFileCommand { FileId = fileId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteFile_WhenUserIsOwner()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var fileMetadata = new FileMetadata { Id = fileId, CreatedBy = userId.ToString(), Url = "http://example.com/file.txt", FileName = "file.txt", ContentType = "text/plain" };
        await _context.FileMetadata.AddAsync(fileMetadata);
        await _context.SaveChangesAsync(CancellationToken.None);

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(false);
        _fileStorageMock.Setup(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _dateTimeMock.Setup(x => x.Now).Returns(now);

        var command = new DeleteFileCommand { FileId = fileId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        fileMetadata.IsDeleted.Should().BeTrue();
        fileMetadata.DeletedDate.Should().Be(now);
        fileMetadata.DeletedBy.Should().Be(userId.ToString());
        _fileStorageMock.Verify(x => x.DeleteFileAsync(fileMetadata.Url, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteFile_WhenUserIsAdmin()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var fileMetadata = new FileMetadata { Id = fileId, CreatedBy = Guid.NewGuid().ToString(), Url = "http://example.com/file.txt", FileName = "file.txt", ContentType = "text/plain" };
        await _context.FileMetadata.AddAsync(fileMetadata);
        await _context.SaveChangesAsync(CancellationToken.None);

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);
        _fileStorageMock.Setup(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _dateTimeMock.Setup(x => x.Now).Returns(now);

        var command = new DeleteFileCommand { FileId = fileId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        fileMetadata.IsDeleted.Should().BeTrue();
        fileMetadata.DeletedDate.Should().Be(now);
        fileMetadata.DeletedBy.Should().Be(userId.ToString());
        _fileStorageMock.Verify(x => x.DeleteFileAsync(fileMetadata.Url, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileStorageDeletionFails()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var fileMetadata = new FileMetadata { Id = fileId, CreatedBy = userId.ToString(), Url = "http://example.com/file.txt", FileName = "file.txt", ContentType = "text/plain" };
        await _context.FileMetadata.AddAsync(fileMetadata);
        await _context.SaveChangesAsync(CancellationToken.None);

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(false);
        _fileStorageMock.Setup(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Storage error", ErrorSources.FileStorage));

        var command = new DeleteFileCommand { FileId = fileId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Storage error");
        result.ErrorSource.Should().Be(ErrorSources.FileStorage);
        fileMetadata.IsDeleted.Should().BeFalse(); // Should not be soft deleted if storage fails
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteFileUsages_WhenFileIsDeleted()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var fileMetadata = new FileMetadata { Id = fileId, CreatedBy = userId.ToString(), Url = "http://example.com/file.txt", FileName = "file.txt", ContentType = "text/plain" };
        fileMetadata.AddFileUsage("Family", Guid.NewGuid());
        fileMetadata.AddFileUsage("Member", Guid.NewGuid());
        await _context.FileMetadata.AddAsync(fileMetadata);
        await _context.SaveChangesAsync(CancellationToken.None);

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(false);
        _fileStorageMock.Setup(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _dateTimeMock.Setup(x => x.Now).Returns(now);

        var command = new DeleteFileCommand { FileId = fileId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        fileMetadata.FileUsages.Should().AllSatisfy(fu =>
        {
            fu.IsDeleted.Should().BeTrue();
            fu.DeletedDate.Should().Be(now);
            fu.DeletedBy.Should().Be(userId.ToString());
        });
    }
}
