using backend.Application.Common.Models;
using backend.Application.MemoryItems.DTOs;
using backend.Domain.Enums;

namespace backend.Application.MemoryItems.Queries.SearchMemoryItems;

public record SearchMemoryItemsQuery : PaginatedQuery, IRequest<PaginatedList<MemoryItemDto>>
{
    public Guid FamilyId { get; set; }
    public string? SearchTerm { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public EmotionalTag? EmotionalTag { get; init; }
    public Guid? MemberId { get; init; }
}
