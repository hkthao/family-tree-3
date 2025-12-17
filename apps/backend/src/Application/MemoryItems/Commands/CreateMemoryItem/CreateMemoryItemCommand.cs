using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.MemoryItems.Commands.CreateMemoryItem;

public record CreateMemoryItemCommand : IRequest<Result<Guid>>
{
    public Guid FamilyId { get; set; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? HappenedAt { get; init; }
    public EmotionalTag EmotionalTag { get; init; } = EmotionalTag.Neutral;
    public ICollection<CreateMemoryMediaCommandDto> Media { get; init; } = [];
    public ICollection<Guid> PersonIds { get; init; } = [];
    public ICollection<Guid> DeletedMediaIds { get; init; } = [];
}

public record CreateMemoryMediaCommandDto
{
    public Guid Id { get; init; }
    public string Url { get; init; } = string.Empty;
}
