using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.FamilyMedias.Commands.UnlinkMediaFromEntity;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using backend.Infrastructure.Data; // For ApplicationDbContext

namespace backend.Application.UnitTests.FamilyMedias.Commands.UnlinkMediaFromEntity;

public class UnlinkMediaFromEntityCommandHandlerTests : TestBase
{
    private readonly UnlinkMediaFromEntityCommandHandler _handler;

    public UnlinkMediaFromEntityCommandHandlerTests()
    {
        // Default authorization setup: allow all family management
        _mockAuthorizationService.Setup(x => x.CanManageFamily(It.IsAny<Guid>())).Returns(true);

        // Handler uses the mocked services and _context from TestBase
        _handler = new UnlinkMediaFromEntityCommandHandler(
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

    private MediaLink CreateTestMediaLink(Guid familyMediaId, RefType refType, Guid refId)
    {
        return new MediaLink
        {
            Id = Guid.NewGuid(),
            FamilyMediaId = familyMediaId,
            RefType = refType,
            RefId = refId
        };
    }

    [Fact]
    public async Task Handle_ShouldUnlinkMediaFromEntity_WhenValidCommand()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);

        var mediaId = Guid.NewGuid();
        var existingMedia = CreateTestFamilyMedia(familyId, mediaId, "test.jpg", MediaType.Image);
        await _context.FamilyMedia.AddAsync(existingMedia);

        var refId = Guid.NewGuid();
        var existingLink = CreateTestMediaLink(mediaId, RefType.Member, refId);
        await _context.MediaLinks.AddAsync(existingLink);
        await _context.SaveChangesAsync();

        var command = new UnlinkMediaFromEntityCommand
        {
            FamilyMediaId = mediaId,
            RefType = RefType.Member,
            RefId = refId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Use a fresh context to verify the deletion
        await using var verificationContext = new ApplicationDbContext(_dbContextOptions, _mockDomainEventDispatcher.Object, _mockUser.Object, _mockDateTime.Object);
        var deletedLink = await verificationContext.MediaLinks.FindAsync(existingLink.Id);
        deletedLink.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyMediaNotFound()
    {
        // Arrange
        var command = new UnlinkMediaFromEntityCommand
        {
            FamilyMediaId = Guid.NewGuid(), // Non-existent media ID
            RefType = RefType.Member,
            RefId = Guid.NewGuid()
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

        var refId = Guid.NewGuid();
        var existingLink = CreateTestMediaLink(mediaId, RefType.Member, refId);
        await _context.MediaLinks.AddAsync(existingLink);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId)).Returns(false); // Deny authorization

        var command = new UnlinkMediaFromEntityCommand
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
    public async Task Handle_ShouldReturnFailure_WhenMediaLinkNotFound()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);

        var mediaId = Guid.NewGuid();
        var existingMedia = CreateTestFamilyMedia(familyId, mediaId, "test.jpg", MediaType.Image);
        await _context.FamilyMedia.AddAsync(existingMedia);
        await _context.SaveChangesAsync();

        var command = new UnlinkMediaFromEntityCommand
        {
            FamilyMediaId = mediaId,
            RefType = RefType.Member,
            RefId = Guid.NewGuid() // Non-existent RefId for link
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
        result.Error.Should().Contain("Media link not found.");
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenInvalidCommand()
    {
        // Arrange
        var command = new UnlinkMediaFromEntityCommand
        {
            FamilyMediaId = Guid.Empty, // Invalid
            RefType = (RefType)99, // Invalid enum value
            RefId = Guid.Empty // Invalid
        };

        // Act
        var validator = new UnlinkMediaFromEntityCommandValidator();
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(UnlinkMediaFromEntityCommand.FamilyMediaId));
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(UnlinkMediaFromEntityCommand.RefType));
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(UnlinkMediaFromEntityCommand.RefId));
    }
}
