using backend.Domain.Common;
using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class MemoryItem : BaseAuditableEntity, ISoftDelete
{
    public Guid FamilyId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime? HappenedAt { get; private set; }
    public EmotionalTag EmotionalTag { get; private set; } = EmotionalTag.Neutral;

    // Navigation properties
    private readonly List<MemoryMedia> _memoryMedia = new();
    public IReadOnlyCollection<MemoryMedia> MemoryMedia => _memoryMedia.AsReadOnly();

    private readonly List<MemoryPerson> _memoryPersons = new();
    public IReadOnlyCollection<MemoryPerson> MemoryPersons => _memoryPersons.AsReadOnly();





    // Private constructor for EF Core
    private MemoryItem() { }

    public MemoryItem(Guid familyId, string title, string? description = null, DateTime? happenedAt = null, EmotionalTag emotionalTag = EmotionalTag.Neutral)
    {
        FamilyId = familyId;
        Title = title;
        Description = description;
        HappenedAt = happenedAt;
        EmotionalTag = emotionalTag;
    }

    public void Update(string title, string? description, DateTime? happenedAt, EmotionalTag emotionalTag)
    {
        Title = title;
        Description = description;
        HappenedAt = happenedAt;
        EmotionalTag = emotionalTag;
    }

    public void AddMedia(MemoryMedia media)
    {
        if (!_memoryMedia.Contains(media))
        {
            _memoryMedia.Add(media);
        }
    }

    public void RemoveMedia(MemoryMedia media)
    {
        _memoryMedia.Remove(media);
    }

    public void AddPerson(MemoryPerson person)
    {
        if (!_memoryPersons.Any(mp => mp.MemberId == person.MemberId))
        {
            _memoryPersons.Add(person);
        }
    }
}
