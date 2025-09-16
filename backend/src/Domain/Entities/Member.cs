
namespace backend.Domain.Entities;

public class Member : BaseAuditableEntity
{
    public string? FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfDeath { get; set; }
    public string? Status { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int Generation { get; set; }
    public int DisplayOrder { get; set; }
    public int FamilyId { get; set; }
    public Family Family { get; set; } = null!;
    public string? Description { get; set; }
}
