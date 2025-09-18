using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backend.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query; // Added back
using Moq;
using Xunit;

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
    [InlineData(1, 3, false, true)]
    [InlineData(2, 3, true, true)]
    [InlineData(4, 3, true, false)]
    [InlineData(1, 10, false, false)]
    public void HasPreviousAndNextPage_ShouldReturnCorrectValues(int pageNumber, int pageSize, bool expectedHasPrevious, bool expectedHasNext)
    {
        var items = new List<string> { "Item1", "Item2", "Item3" };
        var count = 10;
        var paginatedList = new PaginatedList<string>(items, count, pageNumber, pageSize);

        Assert.Equal(expectedHasPrevious, paginatedList.HasPreviousPage);
        Assert.Equal(expectedHasNext, paginatedList.HasNextPage);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnPaginatedListCorrectly()
    {
        var data = Enumerable.Range(1, 10).Select(i => $"Item{i}").ToList();
        int pageNumber = 2;
        int pageSize = 3;

        var queryable = new TestAsyncEnumerable<string>(data.AsQueryable());

        var paginatedList = await PaginatedList<string>.CreateAsync(queryable, pageNumber, pageSize, CancellationToken.None);

        Assert.Equal(pageNumber, paginatedList.PageNumber);
        Assert.Equal(10, paginatedList.TotalCount);
        Assert.Equal(4, paginatedList.TotalPages);
        Assert.Equal(new[] { "Item4", "Item5", "Item6" }, paginatedList.Items);
        Assert.True(paginatedList.HasPreviousPage);
        Assert.True(paginatedList.HasNextPage);
    }

    // --- Helper classes for async IQueryable ---
    private class TestAsyncEnumerable<T> : IAsyncEnumerable<T>, IQueryable<T>
    {
        private readonly IQueryable<T> _queryable;

        public TestAsyncEnumerable(IQueryable<T> queryable) => _queryable = queryable;

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(_queryable.GetEnumerator());

        public Type ElementType => _queryable.ElementType;
        public System.Linq.Expressions.Expression Expression => _queryable.Expression;
        public IQueryProvider Provider => new TestAsyncQueryProvider<T>(_queryable.Provider);

        public IEnumerator<T> GetEnumerator() => _queryable.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _queryable.GetEnumerator();
    }

    private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());
    }

    private class TestAsyncQueryProvider<T> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
            => new TestAsyncEnumerable<T>(_inner.CreateQuery<T>(expression));

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
            => new TestAsyncEnumerable<TElement>(_inner.CreateQuery<TElement>(expression));

        public object? Execute(System.Linq.Expressions.Expression expression) => _inner.Execute(expression);
        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression) => _inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = _inner.Execute(expression);
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType)
                .Invoke(null, new[] { executionResult })!;
        }
    }
}
