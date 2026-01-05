using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.MemoryItems.Commands.UpdateMemoryItem;

public record UpdateMemoryItemCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public Guid FamilyId { get; set; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? HappenedAt { get; init; }
    public EmotionalTag EmotionalTag { get; init; } = EmotionalTag.Neutral;
    public ICollection<Guid> DeletedMediaIds { get; init; } = [];
    public ICollection<UpdateMemoryMediaCommandDto> MemoryMedia { get; init; } = [];
    public ICollection<Guid> PersonIds { get; init; } = [];
    public string? Location { get; set; }
    public Guid? LocationId { get; set; }
}

public record UpdateMemoryMediaCommandDto
{
    public Guid? Id { get; init; } // Null for new media, value for existing media
    public string Url { get; init; } = string.Empty;
}
