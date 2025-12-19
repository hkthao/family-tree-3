using backend.Domain.Enums;

namespace backend.Application.MemoryItems.DTOs;

public class MemoryItemDto
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? HappenedAt { get; set; }
    public EmotionalTag EmotionalTag { get; set; }
    public ICollection<MemoryMediaDto> MemoryMedia { get; set; } = [];
    public ICollection<MemoryPersonDto> MemoryPersons { get; set; } = [];
}
