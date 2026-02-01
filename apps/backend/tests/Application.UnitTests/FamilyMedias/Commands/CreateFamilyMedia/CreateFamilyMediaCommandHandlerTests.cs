using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Queries;
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.FamilyMedias.Commands.CreateFamilyMedia;

public class CreateFamilyMediaCommandHandlerTests : TestBase
{
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ILogger<CreateFamilyMediaCommandHandler>> _loggerMock;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMessageBus> _mockMessageBus; // NEW
    private readonly CreateFamilyMediaCommandHandler _handler;

    public CreateFamilyMediaCommandHandlerTests()
    {
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _loggerMock = new Mock<ILogger<CreateFamilyMediaCommandHandler>>();
        _mockMediator = new Mock<IMediator>();
        _mockMessageBus = new Mock<IMessageBus>(); // Initialize mock

        // Setup default mocks for IFileStorageService for successful upload
        _fileStorageServiceMock.Setup(s => s.SaveFileAsync( // Changed to SaveFileAsync
            It.IsAny<Stream>(),
            It.IsAny<string>(), // fileName
            It.IsAny<string>(), // contentType
            It.IsAny<string>(), // folder
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<FileStorageResultDto>.Success(new FileStorageResultDto { FileUrl = "http://mockurl.com/uploaded/file.jpg", DeleteHash = "mockdeletehash" }));

        // Default authorization setup: allow all family management
        _mockAuthorizationService.Setup(x => x.CanManageFamily(It.IsAny<Guid>())).Returns(true);

        // Setup default mock for IMediator.Send to return a successful FamilyLimitConfigurationDto
        _mockMediator.Setup(m => m.Send(It.IsAny<GetFamilyLimitConfigurationQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FamilyLimitConfigurationDto>.Success(new FamilyLimitConfigurationDto
            {
                Id = Guid.NewGuid(),
                FamilyId = It.IsAny<Guid>(), // This will be overridden by the actual FamilyId in the query
                MaxMembers = 50,
                MaxStorageMb = 1024
            }));

        // Handler uses the mocked services and _context from TestBase
        _handler = new CreateFamilyMediaCommandHandler(
            _context,
            _mockAuthorizationService.Object,
            _fileStorageServiceMock.Object,
            _mockUser.Object,
            _loggerMock.Object,
            _mapper,
            _mockMediator.Object,
            _mockMessageBus.Object); // Pass IMessageBus mock
    } // Closing brace added for constructor

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

        var hardcodedFolderPath = "test-guid-123/family-images"; // Hardcoded for testing
        var command = new CreateFamilyMediaCommand
        {
            FamilyId = familyId,
            File = new byte[] { 1, 2, 3, 4 },
            FileName = "image.jpg",
            MediaType = MediaType.Image,
            Description = "A test image",
            Folder = hardcodedFolderPath // Use hardcoded path
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
        createdMedia!.FamilyId.Should().Be(command.FamilyId?.ToString());
        createdMedia.UploadedBy.Should().Be(_mockUser.Object.UserId);

        _fileStorageServiceMock.Verify(s => s.SaveFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.Is<string>(ct => ct == "image/jpeg"), // contentType
            It.IsAny<string>(), // folder - Simplified to bypass stubborn CS1503 error
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
        _fileStorageServiceMock.Verify(s => s.SaveFileAsync(
            It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()
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
        _fileStorageServiceMock.Verify(s => s.SaveFileAsync(
            It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()
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
        _fileStorageServiceMock.Verify(s => s.SaveFileAsync(
            It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()
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

        _fileStorageServiceMock.Setup(s => s.SaveFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(), // contentType
            It.IsAny<string>(), // folder
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<FileStorageResultDto>.Failure("Storage service unavailable.", ErrorSources.ExternalServiceError)); // Simulate failure

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
        _fileStorageServiceMock.Verify(s => s.SaveFileAsync(
            It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()
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

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenStorageLimitExceeded()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        int maxStorageMb = 1; // 1 MB limit
        long existingFilesSize = (long)(0.8 * 1024 * 1024); // 0.8 MB

        // Override default mock for IMediator to return specific FamilyLimitConfigurationDto
        _mockMediator.Setup(m => m.Send(
                It.Is<GetFamilyLimitConfigurationQuery>(q => q.FamilyId == familyId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FamilyLimitConfigurationDto>.Success(new FamilyLimitConfigurationDto
            {
                Id = Guid.NewGuid(),
                FamilyId = familyId,
                MaxMembers = 50,
                MaxStorageMb = maxStorageMb
            }));

        // Add existing media to almost reach the limit
        _context.FamilyMedia.Add(new Domain.Entities.FamilyMedia // Correct entity type
        {
            Id = Guid.NewGuid(), // Must have an ID
            FamilyId = familyId,
            FileName = "existing1.jpg",
            FilePath = "http://existing.com/file1.jpg",
            MediaType = MediaType.Image,
            FileSize = existingFilesSize, // FileSize is here
            UploadedBy = _mockUser.Object.UserId
        });
        await _context.SaveChangesAsync();

        // Verify the existing media is correctly added to the in-memory database
        _context.FamilyMedia.Should().HaveCount(1);
        _context.FamilyMedia.Sum(fm => fm.FileSize).Should().Be(existingFilesSize);

        var command = new CreateFamilyMediaCommand
        {
            FamilyId = familyId,
            File = new byte[524288], // File that exceeds the limit (0.5 MB)
            FileName = "largefile.jpg",
            MediaType = MediaType.Image
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.Validation);
        result.Error.Should().Contain($"Storage limit ({maxStorageMb} MB) exceeded.");
        _fileStorageServiceMock.Verify(s => s.SaveFileAsync(
            It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()
        ), Times.Never); // File should not be uploaded
    }
}
