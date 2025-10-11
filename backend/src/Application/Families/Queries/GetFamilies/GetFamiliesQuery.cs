using backend.Application.Common.Models;

namespace backend.Application.Families.Queries.GetFamilies
{
    public record class GetFamiliesQuery : PaginatedQuery, IRequest<Result<IReadOnlyList<FamilyListDto>>>
    {
        public string? SearchTerm { get; init; }
    }
}
