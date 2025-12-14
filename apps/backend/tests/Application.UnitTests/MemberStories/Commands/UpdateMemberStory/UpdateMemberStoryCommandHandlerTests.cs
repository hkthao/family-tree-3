using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // Added for Result
using backend.Application.Files.Commands.UploadFileFromUrl; // Added
using backend.Application.Files.DTOs; // Added
using backend.Application.MemberStories.Commands.UpdateMemberStory;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore; // Added for .Include
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.MemberStories.Commands.UpdateMemberStory;

public class UpdateMemberStoryCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IStringLocalizer<UpdateMemberStoryCommandHandler>> _localizerMock;
    private readonly UpdateMemberStoryCommandHandler _handler;

    private readonly Mock<IMediator> _mediatorMock;

    public UpdateMemberStoryCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _localizerMock = new Mock<IStringLocalizer<UpdateMemberStoryCommandHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _handler = new UpdateMemberStoryCommandHandler(_context, _authorizationServiceMock.Object, _localizerMock.Object, _mediatorMock.Object, new Mock<ILogger<UpdateMemberStoryCommandHandler>>().Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateMemberStoryAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var memberStoryId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member = new Member("John", "Doe", "JD", familyId, false) { Id = memberId };
        var memberStory = new MemberStory
        {
            Id = memberStoryId,
            MemberId = memberId,
            Title = "Original Title",
            Story = "Original Story",
            Year = 2000,
            TimeRangeDescription = "Start of the millennium",
            LifeStage = LifeStage.Adulthood,
            Location = "Someplace"
        };

        _context.Families.Add(family);
        _context.Members.Add(member);
        _context.MemberStories.Add(memberStory);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(memberId)).Returns(true);

        var command = new UpdateMemberStoryCommand
        {
            Id = memberStoryId,
            MemberId = memberId,
            Title = "Updated Title",
            Story = "Updated Story Content",
            Year = 2005,
            TimeRangeDescription = "Mid-decade",
            LifeStage = LifeStage.SignificantEvents,
            Location = "Another Place"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedMemberStory = await _context.MemberStories.FindAsync(memberStoryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedMemberStory.Should().NotBeNull();
        updatedMemberStory!.Title.Should().Be(command.Title);
        updatedMemberStory.Story.Should().Be(command.Story);
        updatedMemberStory.Year.Should().Be(command.Year);
        updatedMemberStory.TimeRangeDescription.Should().Be(command.TimeRangeDescription);
        updatedMemberStory.LifeStage.Should().Be(command.LifeStage);
        updatedMemberStory.Location.Should().Be(command.Location);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberStoryNotFound()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var command = new UpdateMemberStoryCommand { Id = Guid.NewGuid(), MemberId = memberId };
        _authorizationServiceMock.Setup(x => x.CanManageFamily(memberId)).Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, $"MemberStory with ID {command.Id}"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var memberStoryId = Guid.NewGuid();
        var memberStory = new MemberStory
        {
            Id = memberStoryId,
            MemberId = memberId,
            Title = "Original Title",
            Story = "Original Story",
            LifeStage = LifeStage.Adulthood
        }; // Missing closing brace here.

        _context.MemberStories.Add(memberStory);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(memberId)).Returns(false);

        var command = new UpdateMemberStoryCommand { Id = memberStoryId, MemberId = memberId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldUpdateMemberStoryImages_WhenProvided()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var memberStoryId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member = new Member("John", "Doe", "JD", familyId, false) { Id = memberId };

        var initialImageUrl1 = "http://permanent.com/initial1.jpg";
        var initialImageUrl2 = "http://permanent.com/initial2.png";

        var memberStory = new MemberStory
        {
            Id = memberStoryId,
            MemberId = memberId,
            Title = "Original Story with Images",
            Story = "Original Content.",
            Year = 2000,
            LifeStage = LifeStage.Adulthood,
            Location = "Someplace",
            MemberStoryImages = new List<MemberStoryImage>
            {
                new MemberStoryImage { ImageUrl = initialImageUrl1 },
                new MemberStoryImage { ImageUrl = initialImageUrl2 }
            }
        };

        _context.Families.Add(family);
        _context.Members.Add(member);
        _context.MemberStories.Add(memberStory);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(memberId)).Returns(true);

        var tempOriginalUrl3 = "http://temp.com/temp/original3.jpg";
        var permanentOriginalUrl3 = "http://permanent.com/original3.jpg";

        var newImageUrl4 = "http://permanent.com/new4.gif"; // Not a temp URL

        // Setup mediator mocks for image uploads
        _mediatorMock.Setup(m => m.Send(It.Is<UploadFileFromUrlCommand>(
            cmd => cmd.FileUrl == tempOriginalUrl3), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ImageUploadResponseDto>.Success(new ImageUploadResponseDto { Url = permanentOriginalUrl3 }));


        var command = new UpdateMemberStoryCommand
        {
            Id = memberStoryId,
            MemberId = memberId,
            Title = "Updated Story with Images",
            Story = "Updated Content.",
            Year = 2005,
            LifeStage = LifeStage.Adulthood,
            Location = "New Place",
            MemberStoryImageUrls = new List<string>
            {
                initialImageUrl1, // Keep initialImageUrl1
                newImageUrl4,     // Add newImageUrl4 (non-temp)
                tempOriginalUrl3  // Add tempOriginalUrl3 (temp)
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedStory = await _context.MemberStories.Include(ms => ms.MemberStoryImages).FirstOrDefaultAsync(ms => ms.Id == memberStoryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedStory.Should().NotBeNull();
        updatedStory!.MemberStoryImages.Should().HaveCount(3); // initial1, new4, permanent3

        // Check initialImageUrl1 is still there
        updatedStory.MemberStoryImages.Should().ContainSingle(img => img.ImageUrl == initialImageUrl1);

        // Check newImageUrl4 is added
        updatedStory.MemberStoryImages.Should().ContainSingle(img => img.ImageUrl == newImageUrl4);

        // Check tempOriginalUrl3 is uploaded and added
        updatedStory.MemberStoryImages.Should().ContainSingle(img => img.ImageUrl == permanentOriginalUrl3);

        // Verify mediator calls for only temp URLs
        _mediatorMock.Verify(m => m.Send(It.Is<UploadFileFromUrlCommand>(cmd => cmd.FileUrl == tempOriginalUrl3), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<UploadFileFromUrlCommand>(), It.IsAny<CancellationToken>()), Times.Once); // Total calls
    }
}
