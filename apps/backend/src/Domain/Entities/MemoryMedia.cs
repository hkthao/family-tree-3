using backend.Domain.Common;
using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class MemoryMedia : BaseEntity
{
    public Guid MemoryItemId { get; private set; }
    public MemoryMediaType MediaType { get; private set; }
    public string Url { get; private set; } = string.Empty;

    // Navigation property
    public virtual MemoryItem MemoryItem { get; private set; } = null!;

    // Private constructor for EF Core
    private MemoryMedia() { }

    public MemoryMedia(Guid memoryItemId, MemoryMediaType mediaType, string url)
    {
        MemoryItemId = memoryItemId;
        MediaType = mediaType;
        Url = url;
    }

    public void Update(MemoryMediaType mediaType, string url)
    {
        MediaType = mediaType;
        Url = url;
    }
}
