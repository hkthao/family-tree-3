using backend.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace backend.Infrastructure.Persistence.Extensions;

public static class ModelBuilderExtensions
{
    public static void AddSoftDeleteQueryFilter(this IMutableEntityType entityType)
    {
        if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
        {
            entityType.SetQueryFilter(BuildSoftDeleteFilter(entityType.ClrType));
        }
    }

    private static LambdaExpression BuildSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "entity");
        var property = Expression.PropertyOrField(parameter, nameof(ISoftDelete.IsDeleted));
        var filter = Expression.Equal(property, Expression.Constant(false));
        return Expression.Lambda(filter, parameter);
    }
}
