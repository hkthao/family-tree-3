using backend.Application.Common.Constants;
using backend.Application.FamilyMedias.Commands.LinkMediaToEntity;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.FamilyMedias.Commands.LinkMediaToEntity;

public class LinkMediaToEntityCommandHandlerTests : TestBase
{
    private readonly LinkMediaToEntityCommandHandler _handler;

    public LinkMediaToEntityCommandHandlerTests()
    {
        // Default authorization setup: allow all family management
        _mockAuthorizationService.Setup(x => x.CanManageFamily(It.IsAny<Guid>())).Returns(true);

        // Handler uses the mocked services and _context from TestBase
        _handler = new LinkMediaToEntityCommandHandler(
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
    public async Task Handle_ShouldLinkMediaToEntity_WhenValidCommand()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);

        var mediaId = Guid.NewGuid();
        var existingMedia = CreateTestFamilyMedia(familyId, mediaId, "test.jpg", MediaType.Image);
        await _context.FamilyMedia.AddAsync(existingMedia);
        await _context.SaveChangesAsync();

        var refId = Guid.NewGuid();
        var command = new LinkMediaToEntityCommand
        {
            FamilyMediaId = mediaId,
            RefType = RefType.Member,
            RefId = refId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty(); // Returns the ID of the new MediaLink

        var createdLink = await _context.MediaLinks.FindAsync(result.Value);
        createdLink.Should().NotBeNull();
        createdLink!.FamilyMediaId.Should().Be(mediaId);
        createdLink.RefType.Should().Be(command.RefType);
        createdLink.RefId.Should().Be(command.RefId);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyMediaNotFound()
    {
        // Arrange
        var refId = Guid.NewGuid();
        var command = new LinkMediaToEntityCommand
        {
            FamilyMediaId = Guid.NewGuid(), // Non-existent media ID
            RefType = RefType.Member,
            RefId = refId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
        result.Error.Should().Contain("FamilyMedia with ID");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAuthorizationFails()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);

        var mediaId = Guid.NewGuid();
        var existingMedia = CreateTestFamilyMedia(familyId, mediaId, "test.jpg", MediaType.Image);
        await _context.FamilyMedia.AddAsync(existingMedia);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId)).Returns(false); // Deny authorization

        var refId = Guid.NewGuid();
        var command = new LinkMediaToEntityCommand
        {
            FamilyMediaId = mediaId,
            RefType = RefType.Member,
            RefId = refId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        result.Error.Should().Be(ErrorMessages.AccessDenied);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenLinkAlreadyExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);

        var mediaId = Guid.NewGuid();
        var existingMedia = CreateTestFamilyMedia(familyId, mediaId, "test.jpg", MediaType.Image);
        await _context.FamilyMedia.AddAsync(existingMedia);

        var refId = Guid.NewGuid();
        var existingLink = new MediaLink
        {
            Id = Guid.NewGuid(),
            FamilyMediaId = mediaId,
            RefType = RefType.Member,
            RefId = refId
        };
        await _context.MediaLinks.AddAsync(existingLink);
        await _context.SaveChangesAsync();

        var command = new LinkMediaToEntityCommand
        {
            FamilyMediaId = mediaId,
            RefType = RefType.Member,
            RefId = refId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.Conflict);
        result.Error.Should().Contain("already linked");
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenInvalidCommand()
    {
        // Arrange
        var command = new LinkMediaToEntityCommand
        {
            FamilyMediaId = Guid.Empty, // Invalid
            RefType = (RefType)99, // Invalid enum value
            RefId = Guid.Empty // Invalid
        };

        // Act
        var validator = new LinkMediaToEntityCommandValidator();
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(LinkMediaToEntityCommand.FamilyMediaId));
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(LinkMediaToEntityCommand.RefType));
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(LinkMediaToEntityCommand.RefId));
    }
}
