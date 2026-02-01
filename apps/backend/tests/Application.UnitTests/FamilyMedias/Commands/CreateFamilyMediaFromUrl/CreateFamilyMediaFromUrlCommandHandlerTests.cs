using backend.Application.Common.Constants;
using backend.Application.FamilyMedias.Commands.CreateFamilyMediaFromUrl;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.FamilyMedias.Commands.CreateFamilyMediaFromUrl;

public class CreateFamilyMediaFromUrlCommandHandlerTests : TestBase
{
    public CreateFamilyMediaFromUrlCommandHandlerTests()
    {
        // Default authorization setup: allow all family management
        _mockAuthorizationService.Setup(x => x.CanManageFamily(It.IsAny<Guid>())).Returns(true);
    }

    private Family CreateTestFamily(Guid familyId)
    {
        var family = Family.Create("Test Family", "TF", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        return family;
    }

    [Fact]
    public async Task Handle_ShouldCreateFamilyMediaFromUrl_WhenValidCommand()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        var command = new CreateFamilyMediaFromUrlCommand
        {
            FamilyId = familyId,
            Url = "https://example.com/images/test.jpg",
            FileName = "test_image.jpg",
            MediaType = MediaType.Image,
        }; // Corrected: Closing brace for command

        var handler = new CreateFamilyMediaFromUrlCommandHandler(_context, _mockAuthorizationService.Object, _mockUser.Object, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FileName.Should().Be(command.FileName);
        result.Value.FilePath.Should().Be(command.Url);
        result.Value.MediaType.Should().Be(command.MediaType);
        result.Value.FileSize.Should().Be(0); // URL-based media has 0 file size

        var createdMedia = await _context.FamilyMedia.FindAsync(result.Value.Id);
        createdMedia.Should().NotBeNull();
        createdMedia!.FamilyId.Should().Be(command.FamilyId);

    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAuthorizationFails()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId)).Returns(false); // Deny authorization

        var command = new CreateFamilyMediaFromUrlCommand
        {
            FamilyId = familyId,
            Url = "https://example.com/images/test.jpg",
            FileName = "test_image.jpg",
            MediaType = MediaType.Image
        };

        var handler = new CreateFamilyMediaFromUrlCommandHandler(_context, _mockAuthorizationService.Object, _mockUser.Object, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        result.Error.Should().Be(ErrorMessages.AccessDenied);

        _context.FamilyMedia.Should().BeEmpty(); // No media should be saved to DB
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenUrlIsEmpty()
    {
        // Arrange
        var command = new CreateFamilyMediaFromUrlCommand
        {
            FamilyId = Guid.NewGuid(),
            Url = "", // Empty URL
            FileName = "test_image.jpg",
            MediaType = MediaType.Image
        };

        var validator = new CreateFamilyMediaFromUrlCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFamilyMediaFromUrlCommand.Url) && e.ErrorMessage == "URL is required.");
        _context.FamilyMedia.Should().BeEmpty(); // No media should be saved to DB
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenUrlIsInvalid()
    {
        // Arrange
        var command = new CreateFamilyMediaFromUrlCommand
        {
            FamilyId = Guid.NewGuid(),
            Url = "not-a-valid-url", // Invalid URL
            FileName = "test_image.jpg",
            MediaType = MediaType.Image
        };

        var handler = new CreateFamilyMediaFromUrlCommandHandler(_context, _mockAuthorizationService.Object, _mockUser.Object, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.Validation);
        result.Error.Should().Contain("URL must be a valid absolute URL."); // Message from validator
        _context.FamilyMedia.Should().BeEmpty(); // No media should be saved to DB
    }

    [Theory]
    [InlineData("https://example.com/image.jpg", MediaType.Image)]
    [InlineData("https://example.com/video.mp4", MediaType.Video)]
    [InlineData("https://example.com/document.pdf", MediaType.Document)]
    [InlineData("https://example.com/unknown.xyz", MediaType.Other)]
    public async Task Handle_ShouldInferMediaTypeFromUrl_WhenMediaTypeIsNotProvided(string url, MediaType expectedMediaType)
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        var command = new CreateFamilyMediaFromUrlCommand
        {
            FamilyId = familyId,
            Url = url,
            FileName = "inferred_file",
            // MediaType is null, should be inferred
        };

        var handler = new CreateFamilyMediaFromUrlCommandHandler(_context, _mockAuthorizationService.Object, _mockUser.Object, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.MediaType.Should().Be(expectedMediaType);
        result.Value.FilePath.Should().Be(command.Url);
    }

    [Fact]
    public async Task Handle_ShouldPrioritizeProvidedMediaType_OverInferredType()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = CreateTestFamily(familyId);
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        var command = new CreateFamilyMediaFromUrlCommand
        {
            FamilyId = familyId,
            Url = "https://example.com/video.mp4", // This would infer Video
            FileName = "explicit_image.jpg",
            MediaType = MediaType.Image, // Explicitly provided as Image
        };

        var handler = new CreateFamilyMediaFromUrlCommandHandler(_context, _mockAuthorizationService.Object, _mockUser.Object, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.MediaType.Should().Be(MediaType.Image); // Should use provided type, not inferred
        result.Value.FilePath.Should().Be(command.Url);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenFileNameIsEmpty()
    {
        // Arrange
        var command = new CreateFamilyMediaFromUrlCommand
        {
            FamilyId = Guid.NewGuid(),
            Url = "https://example.com/some/file.jpg",
            FileName = "", // Empty file name
            MediaType = MediaType.Image
        };

        var validator = new CreateFamilyMediaFromUrlCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFamilyMediaFromUrlCommand.FileName) && e.ErrorMessage == "File name is required.");
        _context.FamilyMedia.Should().BeEmpty(); // No media should be saved to DB
    }
}
