using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia;
using backend.Application.FamilyMedias.DTOs;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.FamilyMedias.Commands.CreateFamilyMedia;

public class CreateFamilyMediaCommandHandlerTests : TestBase
{
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ILogger<CreateFamilyMediaCommandHandler>> _loggerMock;
    private readonly CreateFamilyMediaCommandHandler _handler;

    public CreateFamilyMediaCommandHandlerTests()
    {
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _loggerMock = new Mock<ILogger<CreateFamilyMediaCommandHandler>>();

        // Setup default mocks for IFileStorageService for successful upload
        _fileStorageServiceMock.Setup(s => s.UploadFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<string>.Success("http://mockurl.com/uploaded/file.jpg"));

        // Default authorization setup: allow all family management
        _mockAuthorizationService.Setup(x => x.CanManageFamily(It.IsAny<Guid>())).Returns(true);

        // Handler uses the mocked services and _context from TestBase
        _handler = new CreateFamilyMediaCommandHandler(
            _context,
            _mockAuthorizationService.Object,
            _fileStorageServiceMock.Object,
            _mockUser.Object,
            _loggerMock.Object,
            _mapper);
    }

    private Family CreateTestFamily(Guid familyId)
    {
        var family = Family.Create("Test Family", "TF", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        return family;
    }

    [Fact]
    public async Task Handle_ShouldCreateFamilyMedia_WhenValidCommand()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        var command = new CreateFamilyMediaCommand
        {
            FamilyId = familyId,
            File = new byte[] { 1, 2, 3, 4 },
            FileName = "image.jpg",
            MediaType = MediaType.Image,
            Description = "A test image",
            Folder = "photos"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FileName.Should().Be(command.FileName);
        result.Value.FilePath.Should().StartWith("http://mockurl.com");
        result.Value.MediaType.Should().Be(command.MediaType);
        result.Value.FileSize.Should().Be(command.File.Length);

        var createdMedia = await _context.FamilyMedia.FindAsync(result.Value.Id);
        createdMedia.Should().NotBeNull();
        createdMedia!.FamilyId.Should().Be(command.FamilyId);
        createdMedia.UploadedBy.Should().Be(_mockUser.Object.UserId);

        _fileStorageServiceMock.Verify(s => s.UploadFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.Is<string>(f => f.Contains($"family-media/{familyId}/photos")),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAuthorizationFails()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId)).Returns(false); // Deny authorization

        var command = new CreateFamilyMediaCommand
        {
            FamilyId = familyId,
            File = new byte[] { 1, 2, 3, 4 },
            FileName = "image.jpg",
            MediaType = MediaType.Image
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        _fileStorageServiceMock.Verify(s => s.UploadFileAsync(
            It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()
        ), Times.Never); // File should not be uploaded
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenFileIsEmpty()
    {
        // Arrange
        var command = new CreateFamilyMediaCommand
        {
            FamilyId = Guid.NewGuid(),
            File = new byte[0], // Empty file
            FileName = "image.jpg"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.Validation);
        result.Error.Should().Contain("File content is empty.");
        _fileStorageServiceMock.Verify(s => s.UploadFileAsync(
            It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenFileNameIsEmpty()
    {
        // Arrange
        var command = new CreateFamilyMediaCommand
        {
            FamilyId = Guid.NewGuid(),
            File = new byte[] { 1, 2, 3 },
            FileName = "" // Empty file name
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.Validation);
        result.Error.Should().Contain("File name is empty.");
        _fileStorageServiceMock.Verify(s => s.UploadFileAsync(
            It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileStorageServiceFails()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        _fileStorageServiceMock.Setup(s => s.UploadFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<string>.Failure("Storage service unavailable.")); // Simulate failure

        var command = new CreateFamilyMediaCommand
        {
            FamilyId = familyId,
            File = new byte[] { 1, 2, 3, 4 },
            FileName = "image.png",
            MediaType = MediaType.Image
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
        result.Error.Should().Contain("Storage service unavailable.");
        _fileStorageServiceMock.Verify(s => s.UploadFileAsync(
            It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()
        ), Times.Once); // Should attempt upload once
        _context.FamilyMedia.Should().BeEmpty(); // No media should be saved to DB
    }

    [Fact]
    public async Task Handle_ShouldInferMediaType_WhenNotProvided()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        var command = new CreateFamilyMediaCommand
        {
            FamilyId = familyId,
            File = new byte[] { 1, 2, 3, 4 },
            FileName = "video.mp4", // Should infer Video type
            Description = "A test video"
            // MediaType is null
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.MediaType.Should().Be(MediaType.Video);
    }
}
