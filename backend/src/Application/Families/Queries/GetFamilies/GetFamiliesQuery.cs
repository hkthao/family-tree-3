namespace backend.Application.Families.Queries.GetFamilies;

public class GetFamiliesQuery : IRequest<IReadOnlyList<FamilyListDto>>
{
    public string? SearchTerm { get; init; }
    public DateTime? CreatedAfter { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}