using backend.Domain.Common;
using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class MemoryMedia : BaseEntity
{
    private MemoryMedia() { }
    public Guid MemoryItemId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public virtual MemoryItem MemoryItem { get; private set; } = null!;
    
    public MemoryMedia(Guid memoryItemId, string url)
    {
        MemoryItemId = memoryItemId;
        Url = url;
    }

    public void Update(string url)
    {
        Url = url;
    }
}
