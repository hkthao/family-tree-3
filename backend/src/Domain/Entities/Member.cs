
namespace backend.Domain.Entities;

public class Member : BaseAuditableEntity
{
    public string FullName { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfDeath { get; set; }
    public DateTime? PlaceOfDeath { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int Generation { get; set; }
    public Guid FamilyId { get; set; }
    public string? Biography { get; set; }
    public string? Metadata { get; set; }
}
