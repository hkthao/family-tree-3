using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class FileMetadata : BaseAuditableEntity
{
    public string FileName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public StorageProvider StorageProvider { get; set; }
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; } // in bytes
    public string UploadedBy { get; set; } = null!; // UserProfile.Id of the uploader
    public string? UsedByEntity { get; set; } // e.g., "UserProfile", "Member", "Family"
    public Guid? UsedById;
}
