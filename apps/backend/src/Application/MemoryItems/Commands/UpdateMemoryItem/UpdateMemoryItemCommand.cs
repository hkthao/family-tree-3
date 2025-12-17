using backend.Application.Common.Models;
using backend.Domain.Enums;
using MediatR;

namespace backend.Application.MemoryItems.Commands.UpdateMemoryItem;

public record UpdateMemoryItemCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public Guid FamilyId { get; set; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? HappenedAt { get; init; }
    public EmotionalTag EmotionalTag { get; init; } = EmotionalTag.Neutral;

    public ICollection<UpdateMemoryMediaCommandDto> Media { get; init; } = new List<UpdateMemoryMediaCommandDto>();
    public ICollection<UpdateMemoryPersonCommandDto> Persons { get; init; } = new List<UpdateMemoryPersonCommandDto>();
}

public record UpdateMemoryMediaCommandDto
{
    public Guid? Id { get; init; } // Null for new media, value for existing media
    public MemoryMediaType MediaType { get; init; }
    public string Url { get; init; } = string.Empty;
}

public record UpdateMemoryPersonCommandDto
{
    public Guid MemberId { get; init; }
}
