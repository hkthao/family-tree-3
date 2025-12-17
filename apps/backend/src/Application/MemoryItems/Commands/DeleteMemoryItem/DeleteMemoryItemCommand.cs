using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.MemoryItems.Commands.DeleteMemoryItem;

public record DeleteMemoryItemCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public Guid FamilyId { get; init; } // For permission checks
}
