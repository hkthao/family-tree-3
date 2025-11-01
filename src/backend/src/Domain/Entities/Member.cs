namespace backend.Domain.Entities;

public class Member : BaseAuditableEntity
{
    public string LastName { get; set; } = null!; // Last name
    public string FirstName { get; set; } = null!; // First name
    public string Code { get; set; } = null!; // New property
    public string FullName => $"{FirstName} {LastName}"; // Full name (derived)
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
    public Family Family { get; set; } = null!;
    public bool IsRoot { get; set; } = false;

    // Relationships
    public ICollection<Relationship> Relationships { get; set; } = new List<Relationship>();
    public ICollection<EventMember> EventMembers { get; set; } = new List<EventMember>();

    public Member() { }

    public Member(string lastName, string firstName, string code, Guid familyId)
    {
        LastName = lastName;
        FirstName = firstName;
        Code = code;
        FamilyId = familyId;
    }
}
