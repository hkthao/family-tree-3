using backend.Domain.Common;

namespace backend.Domain.Entities;

public class FileUsage : BaseEntity, ISoftDelete
{
    public Guid FileMetadataId { get; private set; }
    public FileMetadata FileMetadata { get; private set; } = null!;

    public string EntityType { get; private set; } = null!;
    public Guid EntityId { get; private set; }

    // Private constructor for EF Core
    private FileUsage() { }

    public FileUsage(Guid fileMetadataId, string entityType, Guid entityId)
    {
        FileMetadataId = fileMetadataId;
        EntityType = entityType;
        EntityId = entityId;
    }
}
