using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.MemberStories.Commands.UpdateMemberStory;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Localization;
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
        _handler = new UpdateMemberStoryCommandHandler(_context, _authorizationServiceMock.Object, _localizerMock.Object, _mediatorMock.Object);
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
            IsYearEstimated = false,
            LifeStage = LifeStage.Adulthood,
            Location = "Someplace",
            StorytellerId = memberId,
            CertaintyLevel = CertaintyLevel.Sure
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
            IsYearEstimated = true,
            LifeStage = LifeStage.SignificantEvents,
            Location = "Another Place",
            StorytellerId = Guid.NewGuid(), // Changed storyteller
            CertaintyLevel = CertaintyLevel.Estimated
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
        updatedMemberStory.IsYearEstimated.Should().Be(command.IsYearEstimated);
        updatedMemberStory.LifeStage.Should().Be(command.LifeStage);
        updatedMemberStory.Location.Should().Be(command.Location);
        updatedMemberStory.StorytellerId.Should().Be(command.StorytellerId);
        updatedMemberStory.CertaintyLevel.Should().Be(command.CertaintyLevel);
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
            Year = 2000,
            LifeStage = LifeStage.Adulthood,
            CertaintyLevel = CertaintyLevel.Sure
        };

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
}
