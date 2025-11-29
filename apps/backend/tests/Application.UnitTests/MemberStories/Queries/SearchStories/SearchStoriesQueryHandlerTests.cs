using backend.Application.MemberStories.Queries.SearchStories;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.MemberStories.Queries.SearchStories;

public class SearchStoriesQueryHandlerTests : TestBase
{
    private readonly SearchStoriesQueryHandler _handler;

    public SearchStoriesQueryHandlerTests()
    {
        _handler = new SearchStoriesQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedListOfMemberStoryDto_WithNoFilters()
    {
        // Arrange
        var memberId1 = Guid.NewGuid();
        var memberId2 = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member1 = new Member("John", "Doe", "JD", familyId, false) { Id = memberId1 };
        var member2 = new Member("Jane", "Smith", "JS", familyId, false) { Id = memberId2 };

        var stories = new List<MemberStory>
        {
            new MemberStory { Id = Guid.NewGuid(), MemberId = memberId1, Title = "Story A", Story = "Content A", Created = DateTime.Now.AddDays(-2), Member = member1 },
            new MemberStory { Id = Guid.NewGuid(), MemberId = memberId1, Title = "Story B", Story = "Content B", Created = DateTime.Now.AddDays(-1), Member = member1 },
            new MemberStory { Id = Guid.NewGuid(), MemberId = memberId2, Title = "Story C", Story = "Content C", Created = DateTime.Now.AddDays(-3), Member = member2 }
        };

        _context.Families.Add(family);
        _context.Members.Add(member1);
        _context.Members.Add(member2);
        _context.MemberStories.AddRange(stories);
        await _context.SaveChangesAsync();



        var query = new SearchStoriesQuery { Page = 1, ItemsPerPage = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(3);
        result.Value.TotalItems.Should().Be(3);
        result.Value.Items.First().Title.Should().Be("Story B"); // Default sort by Created Desc
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedListOfMemberStoryDto_WithMemberIdFilter()
    {
        // Arrange
        var memberId1 = Guid.NewGuid();
        var memberId2 = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member1 = new Member("John", "Doe", "JD", familyId, false) { Id = memberId1 };
        var member2 = new Member("Jane", "Smith", "JS", familyId, false) { Id = memberId2 };

        var stories = new List<MemberStory>
        {
            new MemberStory { Id = Guid.NewGuid(), MemberId = memberId1, Title = "Story A", Story = "Content A", Created = DateTime.Now.AddDays(-2), Member = member1 },
            new MemberStory { Id = Guid.NewGuid(), MemberId = memberId1, Title = "Story B", Story = "Content B", Created = DateTime.Now.AddDays(-1), Member = member1 },
            new MemberStory { Id = Guid.NewGuid(), MemberId = memberId2, Title = "Story C", Story = "Content C", Created = DateTime.Now.AddDays(-3), Member = member2 }
        };

        _context.Families.Add(family);
        _context.Members.Add(member1);
        _context.Members.Add(member2);
        _context.MemberStories.AddRange(stories);
        await _context.SaveChangesAsync();



        var query = new SearchStoriesQuery { Page = 1, ItemsPerPage = 10, MemberId = memberId1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalItems.Should().Be(2);
        result.Value.Items.First().Title.Should().Be("Story B"); // Default sort by Created Desc
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedListOfMemberStoryDto_WithSearchQueryFilter()
    {
        // Arrange
        var memberId1 = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member1 = new Member("John", "Doe", "JD", familyId, false) { Id = memberId1 };

        var stories = new List<MemberStory>
        {
            new MemberStory { Id = Guid.NewGuid(), MemberId = memberId1, Title = "Story about John", Story = "Content about John's life.", Created = DateTime.Now.AddDays(-2), Member = member1 },
            new MemberStory { Id = Guid.NewGuid(), MemberId = memberId1, Title = "Another story", Story = "Content about something else.", Created = DateTime.Now.AddDays(-1), Member = member1 }
        };

        _context.Families.Add(family);
        _context.Members.Add(member1);
        _context.MemberStories.AddRange(stories);
        await _context.SaveChangesAsync();



        var query = new SearchStoriesQuery { Page = 1, ItemsPerPage = 10, SearchQuery = "John" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.TotalItems.Should().Be(1);
        result.Value.Items.First().Title.Should().Be("Story about John");
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedListOfMemberStoryDto_WithSorting()
    {
        // Arrange
        var memberId1 = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member1 = new Member("John", "Doe", "JD", familyId, false) { Id = memberId1 };

        var stories = new List<MemberStory>
        {
            new MemberStory { Id = Guid.NewGuid(), MemberId = memberId1, Title = "Story A", Story = "Content A", Created = DateTime.Now.AddDays(-2), Member = member1 },
            new MemberStory { Id = Guid.NewGuid(), MemberId = memberId1, Title = "Story B", Story = "Content B", Created = DateTime.Now.AddDays(-1), Member = member1 }
        };

        _context.Families.Add(family);
        _context.Members.Add(member1);
        _context.MemberStories.AddRange(stories);
        await _context.SaveChangesAsync();



        var query = new SearchStoriesQuery { Page = 1, ItemsPerPage = 10, SortBy = "Title", SortOrder = "asc" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.First().Title.Should().Be("Story A");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenNoResultsFound()
    {
        // Arrange
        var query = new SearchStoriesQuery { Page = 1, ItemsPerPage = 10, SearchQuery = "NonExistent" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalItems.Should().Be(0);
    }
}
