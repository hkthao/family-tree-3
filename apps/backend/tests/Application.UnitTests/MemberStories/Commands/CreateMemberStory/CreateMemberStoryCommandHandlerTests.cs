using backend.Application.AI.DTOs;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Faces.Common; // NEW: For BoundingBoxDto
using backend.Application.Faces.Queries;
using backend.Application.Files.Commands.UploadFileFromUrl;
using backend.Application.MemberFaces.Commands.CreateMemberFace; // NEW: Using new CreateMemberFaceCommand
using backend.Application.MemberStories.Commands.CreateMemberStory;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Events; // NEW: For MemberStoryCreatedWithFacesEvent
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.MemberStories.Commands.CreateMemberStory;

public class CreateMemberStoryCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IStringLocalizer<CreateMemberStoryCommandHandler>> _localizerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<CreateMemberStoryCommandHandler>> _loggerMock;
    private readonly CreateMemberStoryCommandHandler _handler;

    public CreateMemberStoryCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _localizerMock = new Mock<IStringLocalizer<CreateMemberStoryCommandHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<CreateMemberStoryCommandHandler>>();

        _handler = new CreateMemberStoryCommandHandler(
            _context,
            _authorizationServiceMock.Object,
            _localizerMock.Object,
            _mediatorMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateMemberStoryAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member = new Member("John", "Doe", "JD", familyId, false) { Id = memberId };

        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new CreateMemberStoryCommand
        {
            MemberId = memberId,
            Title = "My First Story",
            Story = "This is a wonderful story about John.",
            StoryStyle = MemberStoryStyle.Nostalgic.ToString(),
            Perspective = MemberStoryPerspective.FirstPerson.ToString(),
            OriginalImageUrl = "http://example.com/original.jpg",
            ResizedImageUrl = "http://example.com/resized.jpg",
            RawInput = "User's raw input for AI generation"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var createdStory = await _context.MemberStories
                                         .FirstOrDefaultAsync(s => s.Id == result.Value);
        createdStory.Should().NotBeNull();
        createdStory!.MemberId.Should().Be(memberId);
        createdStory.Title.Should().Be(command.Title);
        createdStory.Story.Should().Be(command.Story);
        createdStory.StoryStyle.Should().Be(command.StoryStyle);
        createdStory.Perspective.Should().Be(command.Perspective);
        createdStory.OriginalImageUrl.Should().Be(command.OriginalImageUrl);
        createdStory.ResizedImageUrl.Should().Be(command.ResizedImageUrl);
        createdStory.RawInput.Should().Be(command.RawInput);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        // Arrange
        var memberId = Guid.NewGuid(); // Member does not exist
        var command = new CreateMemberStoryCommand { MemberId = memberId, Title = "Title", Story = "Story" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, $"Member with ID {memberId}"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var member = new Member("John", "Doe", "JD", familyId, false) { Id = memberId };
        _context.Members.Add(member);
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF1" });
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false); // Not authorized

        var command = new CreateMemberStoryCommand { MemberId = memberId, Title = "Title", Story = "Story" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldUploadImagesFromTempUrls_WhenProvided()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member = new Member("John", "Doe", "JD", familyId, false) { Id = memberId };

        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var tempOriginalUrl = "http://temp.com/temp/original.jpg";
        var permanentOriginalUrl = "http://permanent.com/original.jpg";
        var tempResizedUrl = "http://temp.com/temp/resized.jpg";
        var permanentResizedUrl = "http://permanent.com/resized.jpg";

        _mediatorMock.Setup(m => m.Send(It.Is<UploadFileFromUrlCommand>(
            cmd => cmd.FileUrl == tempOriginalUrl), It.IsAny<CancellationToken>()))
            .ReturnsAsync(backend.Application.Common.Models.Result<backend.Application.AI.DTOs.ImageUploadResponseDto>.Success(new backend.Application.AI.DTOs.ImageUploadResponseDto { Url = permanentOriginalUrl }));

        _mediatorMock.Setup(m => m.Send(It.Is<UploadFileFromUrlCommand>(
            cmd => cmd.FileUrl == tempResizedUrl), It.IsAny<CancellationToken>()))
            .ReturnsAsync(backend.Application.Common.Models.Result<backend.Application.AI.DTOs.ImageUploadResponseDto>.Success(new backend.Application.AI.DTOs.ImageUploadResponseDto { Url = permanentResizedUrl }));

        var command = new CreateMemberStoryCommand
        {
            MemberId = memberId,
            Title = "Story with Temp Images",
            Story = "Content.",
            StoryStyle = MemberStoryStyle.Formal.ToString(),
            Perspective = MemberStoryPerspective.FullyNeutral.ToString(),
            OriginalImageUrl = tempOriginalUrl,
            ResizedImageUrl = tempResizedUrl
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var createdStory = await _context.MemberStories.FindAsync(result.Value);
        createdStory!.OriginalImageUrl.Should().Be(permanentOriginalUrl);
        createdStory.ResizedImageUrl.Should().Be(permanentResizedUrl);
        _mediatorMock.Verify(m => m.Send(It.IsAny<UploadFileFromUrlCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldProcessDetectedFaces_WhenProvided()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member = new Member("John", "Doe", "JD", familyId, false) { Id = memberId };

        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var detectedFaceId = Guid.NewGuid().ToString();
        var tempThumbnailUrl = "http://temp.com/temp/face_thumb.png";
        var permanentThumbnailUrl = "http://permanent.com/face_thumb.png";

        var tempOriginalUrl = "http://temp.com/temp/original_story.jpg"; // Defined locally
        var permanentOriginalUrl = "http://permanent.com/original_story.jpg";
        var tempResizedUrl = "http://temp.com/temp/resized_story.jpg"; // Defined locally
        var permanentResizedUrl = "http://permanent.com/resized_story.jpg";

        var command = new CreateMemberStoryCommand
        {
            MemberId = memberId,
            Title = "Story with Detected Faces",
            Story = "Content.",
            StoryStyle = MemberStoryStyle.Formal.ToString(),
            Perspective = MemberStoryPerspective.FullyNeutral.ToString(),
            OriginalImageUrl = tempOriginalUrl,
            ResizedImageUrl = tempResizedUrl,

            DetectedFaces = new List<DetectedFaceDto>
            {
                new DetectedFaceDto
                {
                    Id = detectedFaceId,
                    BoundingBox = new BoundingBoxDto { X = 10, Y = 20, Width = 30, Height = 40 },
                    Confidence = 0.9f,
                    ThumbnailUrl = tempThumbnailUrl,
                    MemberId = memberId
                },
            }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateMemberFaceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid())); // Return a new GUID for the created MemberFace

        _mediatorMock.Setup(m => m.Send(It.Is<UploadFileFromUrlCommand>(
            cmd => cmd.FileUrl == tempThumbnailUrl), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ImageUploadResponseDto>.Success(new ImageUploadResponseDto { Url = permanentThumbnailUrl }));

        _mediatorMock.Setup(m => m.Send(It.Is<UploadFileFromUrlCommand>(
            cmd => cmd.FileUrl == tempOriginalUrl), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ImageUploadResponseDto>.Success(new ImageUploadResponseDto { Url = permanentOriginalUrl }));

        _mediatorMock.Setup(m => m.Send(It.Is<UploadFileFromUrlCommand>(
            cmd => cmd.FileUrl == tempResizedUrl), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ImageUploadResponseDto>.Success(new ImageUploadResponseDto { Url = permanentResizedUrl }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var createdStory = await _context.MemberStories.FindAsync(result.Value);
        createdStory.Should().NotBeNull();
        createdStory!.OriginalImageUrl.Should().Be(permanentOriginalUrl);
        createdStory.ResizedImageUrl.Should().Be(permanentResizedUrl);

        // Verify that the domain event was added
        createdStory.DomainEvents.Should().ContainSingle(e => e is MemberStoryCreatedWithFacesEvent);
        var createdEvent = createdStory.DomainEvents.OfType<MemberStoryCreatedWithFacesEvent>().First();
        createdEvent.MemberStory.Id.Should().Be(createdStory.Id);
        createdEvent.FacesData.Should().HaveCount(1); // Changed from DetectedFaces
        createdEvent.FacesData.First().Id.Should().Be(command.DetectedFaces.First().Id); // Changed from DetectedFaces

        _mediatorMock.Verify(m => m.Send(It.IsAny<UploadFileFromUrlCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2)); // Verify UploadFileFromTempUrlCommand was sent for Original and Resized images
    }
}
