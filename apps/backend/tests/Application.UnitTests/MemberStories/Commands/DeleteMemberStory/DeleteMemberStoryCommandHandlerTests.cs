using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.MemberStories.Commands.DeleteMemberStory;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore; // NEW
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.MemberStories.Commands.DeleteMemberStory;

public class DeleteMemberStoryCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IStringLocalizer<DeleteMemberStoryCommandHandler>> _localizerMock;
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly DeleteMemberStoryCommandHandler _handler;

    public DeleteMemberStoryCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _localizerMock = new Mock<IStringLocalizer<DeleteMemberStoryCommandHandler>>();
        _dateTimeMock = new Mock<IDateTime>();
        _handler = new DeleteMemberStoryCommandHandler(_context, _authorizationServiceMock.Object, _localizerMock.Object, _dateTimeMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteMemberStoryAndReturnSuccess_WhenAuthorized()
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
            Title = "Story to Delete",
            Story = "Content to delete."
        };

        _context.Families.Add(family);
        _context.Members.Add(member);
        _context.MemberStories.Add(memberStory);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true); // Corrected setup

        var command = new DeleteMemberStoryCommand
        {
            Id = memberStoryId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deletedStory = await _context.MemberStories.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == memberStoryId); // Ignore query filter to find soft-deleted entities
        deletedStory.Should().NotBeNull();
        deletedStory!.IsDeleted.Should().BeTrue();
        deletedStory.DeletedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberStoryNotFound()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var command = new DeleteMemberStoryCommand { Id = Guid.NewGuid() };
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
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var memberStoryId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" }; // NEW
        var member = new Member("John", "Doe", "JD", familyId, false) { Id = memberId }; // NEW
        var memberStory = new MemberStory
        {
            Id = memberStoryId,
            MemberId = memberId,
            Title = "Story to Delete",
            Story = "Content to delete."
        };

        _context.Families.Add(family); // NEW
        _context.Members.Add(member); // NEW
        _context.MemberStories.Add(memberStory);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false); // Corrected setup

        var command = new DeleteMemberStoryCommand { Id = memberStoryId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }
}
