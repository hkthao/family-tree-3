using backend.Application.Common.Models;
using backend.Application.MemoryItems.DTOs;

namespace backend.Application.MemoryItems.Queries.GetMemoryItemDetail;

public record GetMemoryItemDetailQuery : IRequest<Result<MemoryItemDto>>
{
    public Guid Id { get; init; }
}
