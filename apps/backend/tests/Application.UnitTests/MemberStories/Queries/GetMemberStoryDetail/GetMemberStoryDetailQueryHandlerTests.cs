using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.MemberStories.Queries.GetMemberStoryDetail;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.MemberStories.Queries.GetMemberStoryDetail;

    public class GetMemberStoryDetailQueryHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly GetMemberStoryDetailQueryHandler _handler;

    public GetMemberStoryDetailQueryHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _handler = new GetMemberStoryDetailQueryHandler(_context, _mapper, _authorizationServiceMock.Object, _mockUser.Object);
    }
    [Fact]
    public async Task Handle_ShouldReturnMemberStoryDto_WhenAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var memberStoryId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member = new Member("John", "Doe", "JD", familyId, false) { Id = memberId };
        var memberStory = new MemberStory { Id = memberStoryId, MemberId = memberId, Title = "My Story", Story = "Content" };

        _context.Families.Add(family);
        _context.Members.Add(member);
        _context.MemberStories.Add(memberStory);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(true);

        var query = new GetMemberStoryDetailQuery(memberStoryId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(memberStoryId);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberStoryNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var query = new GetMemberStoryDetailQuery(nonExistentId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, $"MemberStory with ID {nonExistentId}"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var memberStoryId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member = new Member("John", "Doe", "JD", familyId, false) { Id = memberId };
        var memberStory = new MemberStory { Id = memberStoryId, MemberId = memberId, Title = "My Story", Story = "Content" };

        _context.Families.Add(family);
        _context.Members.Add(member);
        _context.MemberStories.Add(memberStory);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(false); // Not authorized

        var query = new GetMemberStoryDetailQuery(memberStoryId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }
}
