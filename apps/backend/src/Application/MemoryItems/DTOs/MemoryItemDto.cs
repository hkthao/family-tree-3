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

    public ICollection<MemoryMediaDto> Media { get; set; } = new List<MemoryMediaDto>();
    public ICollection<MemoryPersonDto> Persons { get; set; } = new List<MemoryPersonDto>();

    public DateTime Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}
