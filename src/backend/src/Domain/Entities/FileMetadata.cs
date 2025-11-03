using backend.Domain.Common;
using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class FileMetadata : BaseAuditableEntity, IAggregateRoot
{
    public string FileName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public StorageProvider StorageProvider { get; set; }
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; } // in bytes

    private readonly HashSet<FileUsage> _fileUsages = new();
    public IReadOnlyCollection<FileUsage> FileUsages => _fileUsages;

    public void AddFileUsage(string entityType, Guid entityId)
    {
        if (!_fileUsages.Any(fu => fu.EntityType == entityType && fu.EntityId == entityId))
        {
            _fileUsages.Add(new FileUsage(Id, entityType, entityId));
        }
    }

    public void RemoveFileUsage(string entityType, Guid entityId)
    {
        var fileUsage = _fileUsages.FirstOrDefault(fu => fu.EntityType == entityType && fu.EntityId == entityId);
        if (fileUsage != null)
        {
            _fileUsages.Remove(fileUsage);
        }
    }
}
