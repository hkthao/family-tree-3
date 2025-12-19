using backend.Application.Common.Constants;
using backend.Application.FamilyMedias.Commands.UpdateFamilyMedia;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.FamilyMedias.Commands.UpdateFamilyMedia;

public class UpdateFamilyMediaCommandHandlerTests : TestBase
{
    private readonly UpdateFamilyMediaCommandHandler _handler;

    public UpdateFamilyMediaCommandHandlerTests()
    {
        // Default authorization setup: allow all family management
        _mockAuthorizationService.Setup(x => x.CanManageFamily(It.IsAny<Guid>())).Returns(true);

        // Handler uses the mocked services and _context from TestBase
        _handler = new UpdateFamilyMediaCommandHandler(
            _context,
            _mockAuthorizationService.Object);
    }

    private Family CreateTestFamily(Guid familyId)
    {
        var family = Family.Create("Test Family", "TF", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        return family;
    }

    private FamilyMedia CreateTestFamilyMedia(Guid familyId, Guid mediaId, string fileName, MediaType mediaType)
    {
        return new FamilyMedia
        {
            Id = mediaId,
            FamilyId = familyId,
            FileName = fileName,
            FilePath = "http://test.url/file.jpg",
            MediaType = mediaType,
            FileSize = 100,
            Description = "Initial description",
            UploadedBy = _mockUser.Object.UserId
        };
    }

    [Fact]
    public async Task Handle_ShouldUpdateFamilyMedia_WhenValidCommand()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var mediaId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        var existingMedia = CreateTestFamilyMedia(familyId, mediaId, "old.jpg", MediaType.Image);

        await _context.Families.AddAsync(family);
        await _context.FamilyMedia.AddAsync(existingMedia);
        await _context.SaveChangesAsync();

        var command = new UpdateFamilyMediaCommand
        {
            Id = mediaId,
            FamilyId = familyId,
            FileName = "new.png",
            MediaType = MediaType.Video, // Change type
            Description = "Updated description"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedMedia = await _context.FamilyMedia.FindAsync(mediaId);
        updatedMedia.Should().NotBeNull();
        updatedMedia!.FileName.Should().Be(command.FileName);
        updatedMedia.MediaType.Should().Be(command.MediaType);
        updatedMedia.Description.Should().Be(command.Description);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAuthorizationFails()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var mediaId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        var existingMedia = CreateTestFamilyMedia(familyId, mediaId, "test.jpg", MediaType.Image);

        await _context.Families.AddAsync(family);
        await _context.FamilyMedia.AddAsync(existingMedia);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId)).Returns(false); // Deny authorization

        var command = new UpdateFamilyMediaCommand
        {
            Id = mediaId,
            FamilyId = familyId,
            FileName = "new.jpg",
            MediaType = MediaType.Image
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        result.Error.Should().Be(ErrorMessages.AccessDenied);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyMediaNotFound()
    {
        // Arrange
        var command = new UpdateFamilyMediaCommand
        {
            Id = Guid.NewGuid(), // Non-existent ID
            FamilyId = Guid.NewGuid(),
            FileName = "any.jpg",
            MediaType = MediaType.Image
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
        result.Error.Should().Contain("not found.");
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenFileNameIsEmpty()
    {
        // Arrange
        var command = new UpdateFamilyMediaCommand
        {
            Id = Guid.NewGuid(),
            FamilyId = Guid.NewGuid(),
            FileName = "", // Empty file name
            MediaType = MediaType.Image
        };

        // Act
        var validator = new UpdateFamilyMediaCommandValidator();
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateFamilyMediaCommand.FileName));
    }
}
