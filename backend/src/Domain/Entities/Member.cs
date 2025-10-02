
namespace backend.Domain.Entities;

public class Member : BaseAuditableEntity
{
    public string LastName { get; set; } = null!; // Last name
    public string FirstName { get; set; } = null!; // First name
    public string FullName => $"{LastName} {FirstName}"; // Full name (derived)
    public string? Nickname { get; set; } // New
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfDeath { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? PlaceOfDeath { get; set; }
    public string? Occupation { get; set; } // New
    public string? AvatarUrl { get; set; }
    public string? Biography { get; set; } // New
    public Guid FamilyId { get; set; }


    // Relationships
    public Guid? FatherId { get; set; }
    public Member? Father { get; set; }

    public Guid? MotherId { get; set; }
    public Member? Mother { get; set; }

    public Guid? SpouseId { get; set; } // New
    public Member? Spouse { get; set; }

    public ICollection<Member> Children { get; set; } = new List<Member>();}
