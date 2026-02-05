using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Files;
using backend.Application.Common.Interfaces.Services;
using backend.Application.Common.Models;
using backend.Application.FamilyMedias.Commands.DeleteFamilyMedia;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Infrastructure.Data; // For ApplicationDbContext
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.FamilyMedias.Commands.DeleteFamilyMedia;

public class DeleteFamilyMediaCommandHandlerTests : TestBase
{
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ILogger<DeleteFamilyMediaCommandHandler>> _loggerMock;
    private readonly Mock<IMessageBus> _mockMessageBus; // NEW
    private readonly DeleteFamilyMediaCommandHandler _handler;

    public DeleteFamilyMediaCommandHandlerTests()
    {
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _loggerMock = new Mock<ILogger<DeleteFamilyMediaCommandHandler>>();
        _mockMessageBus = new Mock<IMessageBus>(); // Initialize mock

        // Setup default mocks for IFileStorageService for successful deletion
        _fileStorageServiceMock.Setup(s => s.DeleteFileAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result.Success());

        // Default authorization setup: allow all family management
        _mockAuthorizationService.Setup(x => x.CanManageFamily(It.IsAny<Guid>())).Returns(true);

        // Handler uses the mocked services and _context from TestBase
        _handler = new DeleteFamilyMediaCommandHandler(
            _context,
            _mockAuthorizationService.Object,
            _mockMessageBus.Object, // Pass IMessageBus mock
            _loggerMock.Object);
    }

    private Family CreateTestFamily(Guid familyId)
    {
        var family = Family.Create("Test Family", "TF", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        return family;
    }

    private FamilyMedia CreateTestFamilyMedia(Guid familyId, Guid mediaId, string fileName, MediaType mediaType, string filePath)
    {
        return new FamilyMedia
        {
            Id = mediaId,
            FamilyId = familyId,
            FileName = fileName,
            FilePath = filePath,
            MediaType = mediaType,
            FileSize = 100,
            Description = "Test description",

        };
    }

    [Fact]
    public async Task Handle_ShouldDeleteFamilyMedia_WhenValidCommand()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var mediaId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        var existingMedia = CreateTestFamilyMedia(familyId, mediaId, "test.jpg", MediaType.Image, "http://test.url/file.jpg");

        await _context.Families.AddAsync(family);
        await _context.FamilyMedia.AddAsync(existingMedia);
        await _context.SaveChangesAsync();

        var command = new DeleteFamilyMediaCommand { Id = mediaId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Use a fresh context to verify the deletion
        await using var verificationContext = new ApplicationDbContext(_dbContextOptions, _mockDomainEventDispatcher.Object, _mockUser.Object, _mockDateTime.Object);
        var deletedMedia = await verificationContext.FamilyMedia.FindAsync(mediaId);
        deletedMedia.Should().BeNull(); // Soft delete is handled by interceptor, so it should be null after filtering !fm.IsDeleted
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAuthorizationFails()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var mediaId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        var existingMedia = CreateTestFamilyMedia(familyId, mediaId, "test.jpg", MediaType.Image, "http://test.url/file.jpg");

        await _context.Families.AddAsync(family);
        await _context.FamilyMedia.AddAsync(existingMedia);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId)).Returns(false); // Deny authorization

        var command = new DeleteFamilyMediaCommand { Id = mediaId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        result.Error.Should().Be(ErrorMessages.AccessDenied);


        _context.FamilyMedia.Should().Contain(existingMedia); // Media should still exist
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyMediaNotFound()
    {
        // Arrange
        var command = new DeleteFamilyMediaCommand { Id = Guid.NewGuid() }; // Non-existent ID

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
        result.Error.Should().Contain("not found.");


    }

    [Fact]
    public async Task Handle_ShouldProceedWithDbDeletion_WhenFileStorageServiceFails()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var mediaId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        var existingMedia = CreateTestFamilyMedia(familyId, mediaId, "test.jpg", MediaType.Image, "http://test.url/file.jpg");

        await _context.Families.AddAsync(family);
        await _context.FamilyMedia.AddAsync(existingMedia);
        await _context.SaveChangesAsync();

        _fileStorageServiceMock.Setup(s => s.DeleteFileAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result.Failure("Storage deletion failed.")); // Simulate storage failure

        var command = new DeleteFamilyMediaCommand { Id = mediaId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Should still succeed as per current handler logic



        // Verify DB deletion (soft delete in this case)
        await using var verificationContext = new ApplicationDbContext(_dbContextOptions, _mockDomainEventDispatcher.Object, _mockUser.Object, _mockDateTime.Object);
        var deletedMedia = await verificationContext.FamilyMedia.IgnoreQueryFilters().FirstOrDefaultAsync(fm => fm.Id == mediaId);
        deletedMedia.Should().BeNull();
    }
}
