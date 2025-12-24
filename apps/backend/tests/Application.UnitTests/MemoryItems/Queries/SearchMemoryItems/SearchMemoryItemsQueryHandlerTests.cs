using backend.Application.MemoryItems.Queries.SearchMemoryItems;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.MemoryItems.Queries.SearchMemoryItems;

public class SearchMemoryItemsQueryHandlerTests : TestBase
{
    private readonly SearchMemoryItemsQueryHandler _handler;

    public SearchMemoryItemsQueryHandlerTests()
    {
        _handler = new SearchMemoryItemsQueryHandler(_context, _mapper);
    }

    private async Task SeedData(Guid familyId, Guid memberId)
    {
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "FAM-TEST" });
        _context.Members.Add(new Member("Test", "Member", "TM001", familyId) { Id = memberId });

        var memoryItem1 = new MemoryItem(familyId, "Happy Memory 1", "Description of a happy event.", DateTime.Now.AddDays(-30), EmotionalTag.Happy);
        memoryItem1.AddPerson(new MemoryPerson(memoryItem1.Id, memberId));
        _context.MemoryItems.Add(memoryItem1);

        var memoryItem2 = new MemoryItem(familyId, "Sad Memory 2", "Description of a sad event.", DateTime.Now.AddDays(-20), EmotionalTag.Sad);
        _context.MemoryItems.Add(memoryItem2);

        var memoryItem3 = new MemoryItem(familyId, "Neutral Event 3", "Another description.", DateTime.Now.AddDays(-10), EmotionalTag.Neutral);
        memoryItem3.AddPerson(new MemoryPerson(memoryItem3.Id, memberId));
        _context.MemoryItems.Add(memoryItem3);

        var memoryItem4 = new MemoryItem(familyId, "Happy Memory 4 (Deleted)", "Should not appear.", DateTime.Now.AddDays(-5), EmotionalTag.Happy) { IsDeleted = true };
        _context.MemoryItems.Add(memoryItem4);

        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_FilteredAndSortedCorrectly()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        await SeedData(familyId, memberId);

        var query = new SearchMemoryItemsQuery
        {
            FamilyId = familyId,
            SearchQuery = "memory",
            StartDate = DateTime.Now.AddDays(-35),
            EndDate = DateTime.Now.AddDays(-15),
            EmotionalTag = EmotionalTag.Happy,
            MemberId = memberId,
            SortBy = "happenedAtAsc",
            Page = 1,
            ItemsPerPage = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1); // Should only include "Happy Memory 1"
        result.Items.First().Title.Should().Be("Happy Memory 1");
        result.TotalItems.Should().Be(1);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllItems_WhenNoFiltersApplied()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        await SeedData(familyId, memberId);

        var query = new SearchMemoryItemsQuery
        {
            FamilyId = familyId,
            SortBy = "happenedAtAsc",
            Page = 1,
            ItemsPerPage = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3); // Excluding the soft-deleted item
        result.Items.First().Title.Should().Be("Happy Memory 1");
        result.Items.Last().Title.Should().Be("Neutral Event 3");
    }

    [Fact]
    public async Task Handle_ShouldFilterBySearchTerm()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        await SeedData(familyId, memberId);

        var query = new SearchMemoryItemsQuery
        {
            FamilyId = familyId,
            SearchQuery = "sad",
            Page = 1,
            ItemsPerPage = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Sad Memory 2");
    }

    [Fact]
    public async Task Handle_ShouldFilterByEmotionalTag()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        await SeedData(familyId, memberId);

        var query = new SearchMemoryItemsQuery
        {
            FamilyId = familyId,
            EmotionalTag = EmotionalTag.Neutral,
            Page = 1,
            ItemsPerPage = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Neutral Event 3");
    }

    [Fact]
    public async Task Handle_ShouldFilterByMemberId()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        await SeedData(familyId, memberId);

        var query = new SearchMemoryItemsQuery
        {
            FamilyId = familyId,
            MemberId = memberId,
            Page = 1,
            ItemsPerPage = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2); // memoryItem1 and memoryItem3
        result.Items.Should().Contain(mi => mi.Title == "Happy Memory 1");
        result.Items.Should().Contain(mi => mi.Title == "Neutral Event 3");
    }

    [Fact]
    public async Task Handle_ShouldApplySorting()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        await SeedData(familyId, memberId);

        var query = new SearchMemoryItemsQuery
        {
            FamilyId = familyId,
            SortBy = "titleDesc",
            Page = 1,
            ItemsPerPage = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items.First().Title.Should().Be("Sad Memory 2");
        result.Items.Last().Title.Should().Be("Happy Memory 1");
    }

    [Fact]
    public async Task Handle_ShouldApplyPagination()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        await SeedData(familyId, memberId);

        var query = new SearchMemoryItemsQuery
        {
            FamilyId = familyId,
            SortBy = "happenedAtAsc",
            Page = 1,
            ItemsPerPage = 2
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(3);
        result.TotalPages.Should().Be(2);
        result.Items.First().Title.Should().Be("Happy Memory 1");

        // Get second page
        var queryPage2 = new SearchMemoryItemsQuery
        {
            FamilyId = familyId,
            SortBy = "happenedAtAsc",
            Page = 2,
            ItemsPerPage = 2
        };
        result = await _handler.Handle(queryPage2, CancellationToken.None);
        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Neutral Event 3");
    }
}
