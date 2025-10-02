using System.Collections.Generic;
using Xunit;
using backend.Application.Common.Models;

namespace Application.UnitTests.Common.Models;

public class PaginatedListTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        var items = new List<string> { "Item1", "Item2", "Item3" };
        var count = 10;
        var pageNumber = 2;
        var pageSize = 3;

        var paginatedList = new PaginatedList<string>(items, count, pageNumber, pageSize);

        Assert.Equal(items, paginatedList.Items);
        Assert.Equal(pageNumber, paginatedList.PageNumber);
        Assert.Equal(4, paginatedList.TotalPages);
        Assert.Equal(count, paginatedList.TotalCount);
    }

    [Theory]
    [InlineData(10, 1, 3, 4)] // TotalCount, PageNumber, PageSize, ExpectedTotalPages
    [InlineData(0, 1, 10, 0)]
    [InlineData(10, 1, 10, 1)]
    [InlineData(10, 2, 5, 2)]
    public void TotalPages_ShouldBeCalculatedCorrectly(int totalCount, int pageNumber, int pageSize, int expectedTotalPages)
    {
        var items = new List<string>();
        var paginatedList = new PaginatedList<string>(items, totalCount, pageNumber, pageSize);

        Assert.Equal(expectedTotalPages, paginatedList.TotalPages);
    }
}