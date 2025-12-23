namespace backend.Domain.Entities;

public class MemoryPerson : BaseEntity
{
    public Guid MemoryItemId { get; private set; }
    public Guid MemberId { get; private set; }

    // Navigation properties
    public virtual MemoryItem MemoryItem { get; private set; } = null!;
    public virtual Member Member { get; private set; } = null!;

    // Private constructor for EF Core
    private MemoryPerson() { }

    public MemoryPerson(Guid memoryItemId, Guid memberId)
    {
        MemoryItemId = memoryItemId;
        MemberId = memberId;
    }
}
