using System.Linq.Expressions; // For Expression
using backend.Application.Common.Models;

namespace backend.Application.Common.Extensions;

public static class QueryableExtensions
{
    public static Task<PaginatedList<TDestination>> PaginatedListAsync<TDestination>(this IQueryable<TDestination> queryable, int page, int itemsPerPage) where TDestination : class
        => PaginatedList<TDestination>.CreateAsync(queryable.AsNoTracking(), page, itemsPerPage);

    public static IQueryable<T> OrderByPropertyName<T>(this IQueryable<T> source, string propertyName, bool descending)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return source;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, propertyName);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = descending ? "OrderByDescending" : "OrderBy";

        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), property.Type },
            source.Expression,
            Expression.Quote(lambda));

        return source.Provider.CreateQuery<T>(resultExpression);
    }
    public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> source, string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return source;
        }

        var parts = orderBy.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string propertyName = parts[0];
        bool descending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        return source.OrderByPropertyName(propertyName, descending);
    }
}
