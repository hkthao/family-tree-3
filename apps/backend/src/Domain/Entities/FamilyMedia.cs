using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class FamilyMedia : BaseAuditableEntity, ISoftDelete
{
    public Guid FamilyId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty; // URL or path to storage
    public MediaType MediaType { get; set; }
    public long FileSize { get; set; } // in bytes
    public string? Description { get; set; }
    public Guid? UploadedBy { get; set; } // User who uploaded the file

    // Navigation properties
    public Family Family { get; set; } = default!;
    public ICollection<MediaLink> MediaLinks { get; set; } = [];
}
