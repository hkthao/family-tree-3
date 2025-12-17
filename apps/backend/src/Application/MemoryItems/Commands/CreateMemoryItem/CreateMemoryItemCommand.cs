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

    public ICollection<CreateMemoryMediaCommandDto> Media { get; init; } = new List<CreateMemoryMediaCommandDto>();
    public ICollection<CreateMemoryPersonCommandDto> Persons { get; init; } = new List<CreateMemoryPersonCommandDto>();
}

public record CreateMemoryMediaCommandDto
{
    public MemoryMediaType MediaType { get; init; }
    public string Url { get; init; } = string.Empty;
}

public record CreateMemoryPersonCommandDto
{
    public Guid MemberId { get; init; }
}
