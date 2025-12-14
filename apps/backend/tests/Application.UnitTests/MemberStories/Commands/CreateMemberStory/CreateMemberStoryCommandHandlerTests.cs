using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.Commands.UploadFileFromUrl;
using backend.Application.Files.DTOs;
using backend.Application.MemberStories.Commands.CreateMemberStory;
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // NEW
using backend.Domain.Entities;
using backend.Domain.Enums;
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
            Year = 1990,
            TimeRangeDescription = "Early childhood",
            LifeStage = LifeStage.Childhood,
            Location = "Hanoi, Vietnam"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var createdStory = await _context.MemberStories
                                         .Include(ms => ms.MemberStoryImages) // Include images for assertion
                                         .FirstOrDefaultAsync(s => s.Id == result.Value);
        createdStory.Should().NotBeNull();
        createdStory!.MemberId.Should().Be(memberId);
        createdStory.Title.Should().Be(command.Title);
        createdStory.Story.Should().Be(command.Story);
        createdStory.Year.Should().Be(command.Year);
        createdStory.TimeRangeDescription.Should().Be(command.TimeRangeDescription);
        createdStory.LifeStage.Should().Be(command.LifeStage);
        createdStory.Location.Should().Be(command.Location);
        createdStory.MemberStoryImages.Should().BeEmpty(); // No images provided in this test
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
    public async Task Handle_ShouldUploadMemberStoryImages_WhenProvided()
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

        var tempOriginalUrl1 = "http://temp.com/temp/original1.jpg";
        var permanentOriginalUrl1 = "http://permanent.com/original1.jpg";

        var tempOriginalUrl2 = "http://example.com/images/original2.png";

        // Setup mediator mocks for image uploads
        _mediatorMock.Setup(m => m.Send(It.Is<UploadFileFromUrlCommand>(
            cmd => cmd.FileUrl == tempOriginalUrl1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ImageUploadResponseDto>.Success(new ImageUploadResponseDto { Url = permanentOriginalUrl1 }));


        var command = new CreateMemberStoryCommand
        {
            MemberId = memberId,
            Title = "Story with Multiple Images",
            Story = "Content.",
            Year = 2000,
            LifeStage = LifeStage.Adulthood,
            MemberStoryImageUrls = new List<string>
            {
                tempOriginalUrl1,
                tempOriginalUrl2
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var createdStory = await _context.MemberStories.Include(ms => ms.MemberStoryImages).FirstOrDefaultAsync(ms => ms.Id == result.Value);
        createdStory.Should().NotBeNull();
        createdStory!.MemberStoryImages.Should().HaveCount(2);

        var image1 = createdStory.MemberStoryImages.FirstOrDefault(img => img.ImageUrl == permanentOriginalUrl1);
        image1.Should().NotBeNull();
        image1!.ImageUrl.Should().Be(permanentOriginalUrl1);

        var image2 = createdStory.MemberStoryImages.FirstOrDefault(img => img.ImageUrl == tempOriginalUrl2);
        image2.Should().NotBeNull();
        image2!.ImageUrl.Should().Be(tempOriginalUrl2);

        // Verify mediator calls for only temp URLs
        _mediatorMock.Verify(m => m.Send(It.Is<UploadFileFromUrlCommand>(cmd => cmd.FileUrl == tempOriginalUrl1), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<UploadFileFromUrlCommand>(), It.IsAny<CancellationToken>()), Times.Once); // Total calls
    }


}
