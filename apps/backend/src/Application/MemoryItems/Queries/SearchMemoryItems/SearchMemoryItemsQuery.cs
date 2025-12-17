using backend.Application.Common.Models;
using backend.Application.MemoryItems.DTOs;
using backend.Domain.Enums;
using MediatR;

namespace backend.Application.MemoryItems.Queries.SearchMemoryItems;

public record SearchMemoryItemsQuery : IRequest<PaginatedList<MemoryItemDto>>
{
    public Guid FamilyId { get; init; }
    public string? SearchTerm { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public EmotionalTag? EmotionalTag { get; init; }
    public Guid? MemberId { get; init; } // To filter by persons tagged in memory item
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string OrderBy { get; init; } = "happenedAtDesc"; // Default ordering
}
