using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemberStories.Queries.GetMemoriesByMemberId;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.MemberStories.Queries.GetMemoriesByMemberId;

public class GetMemberStoriesByMemberIdQueryHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly GetMemberStoriesByMemberIdQueryHandler _handler;

    public GetMemberStoriesByMemberIdQueryHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _handler = new GetMemberStoriesByMemberIdQueryHandler(_context, _mapper, _authorizationServiceMock.Object);
    }


    [Fact]
    public async Task Handle_ShouldReturnPaginatedListOfMemberStoryDto_WhenAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var member = new Member("John", "Doe", "JD", familyId, false) { Id = memberId };
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };

        var stories = new List<MemberStory>
        {
            new MemberStory { Id = Guid.NewGuid(), MemberId = memberId, Title = "Story 1", Story = "Content 1", Created = DateTime.Now.AddDays(-2) },
            new MemberStory { Id = Guid.NewGuid(), MemberId = memberId, Title = "Story 2", Story = "Content 2", Created = DateTime.Now.AddDays(-1) },
            new MemberStory { Id = Guid.NewGuid(), MemberId = Guid.NewGuid(), Title = "Other Member Story", Story = "Other Content", Created = DateTime.Now.AddDays(-3) }
        };

        _context.Families.Add(family);
        _context.Members.Add(member);
        _context.MemberStories.AddRange(stories);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(true);




        var query = new GetMemberStoriesByMemberIdQuery(memberId, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalItems.Should().Be(2);
        result.Value.Items.First().Title.Should().Be("Story 2"); // Ordered by Created Desc
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedListOfMemberStoryDto_WithPagination()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var member = new Member("John", "Doe", "JD", familyId, false) { Id = memberId };
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };

        var stories = new List<MemberStory>();
        for (int i = 0; i < 5; i++)
        {
            stories.Add(new MemberStory { Id = Guid.NewGuid(), MemberId = memberId, Title = $"Story {i}", Story = $"Content {i}", Created = DateTime.Now.AddDays(-i) });
        }

        _context.Families.Add(family);
        _context.Members.Add(member);
        _context.MemberStories.AddRange(stories);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(true);




        var query = new GetMemberStoriesByMemberIdQuery(memberId, 2, 2); // Page 2, PageSize 2

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalItems.Should().Be(5);
        result.Value.Items.First().Title.Should().Be("Story 2"); // Should be the 3rd story (index 2 after sorting desc)
    }


    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        // Arrange
        var nonExistentMemberId = Guid.NewGuid();
        var query = new GetMemberStoriesByMemberIdQuery(nonExistentMemberId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, $"Member with ID {nonExistentMemberId}"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var member = new Member("John", "Doe", "JD", familyId, false) { Id = memberId };
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };

        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(false); // Not authorized

        var query = new GetMemberStoriesByMemberIdQuery(memberId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }
}
