using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class FamilyMedia : BaseAuditableEntity
{
    public Guid? FamilyId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty; // URL or path to storage
    public MediaType MediaType { get; set; }
    public long FileSize { get; set; } // in bytes
    public string? Description { get; set; }
    public string? DeleteHash { get; set; }
    public ICollection<MediaLink> MediaLinks { get; set; } = [];
}
