using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class MediaLink : BaseAuditableEntity
{
    public Guid FamilyMediaId { get; set; }
    public RefType RefType { get; set; }
    public Guid RefId { get; set; }
    public MediaLinkType MediaLinkType { get; set; } // NEW

    // Navigation properties
    public FamilyMedia FamilyMedia { get; set; } = default!;
}
