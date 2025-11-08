using backend.Domain.Events.Members;
using backend.Domain.Enums; // Add this for RelationshipType

namespace backend.Domain.Entities;

public class Member : BaseAuditableEntity
{
    public string LastName { get; private set; } = null!; // Last name
    public string FirstName { get; private set; } = null!; // First name
    public string Code { get; private set; } = null!; // New property
    public string FullName => $"{FirstName} {LastName}"; // Full name (derived)
    public string? Nickname { get; private set; } // New
    public string? Gender { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public DateTime? DateOfDeath { get; private set; }
    public string? PlaceOfBirth { get; private set; }
    public string? PlaceOfDeath { get; private set; }
    public string? Occupation { get; private set; } // New
    public string? AvatarUrl { get; private set; }
    public string? Biography { get; private set; } // New
    public Guid FamilyId { get; private set; }
    public Family Family { get; private set; } = null!;
    public bool IsRoot { get; private set; } = false;

    public void SetAsRoot()
    {
        IsRoot = true;
    }

    public void UnsetAsRoot()
    {
        IsRoot = false;
    }

    public void Update(string firstName, string lastName, string code, string? nickname, string? gender, DateTime? dateOfBirth, DateTime? dateOfDeath, string? placeOfBirth, string? placeOfDeath, string? occupation, string? avatarUrl, string? biography)
    {
        FirstName = firstName;
        LastName = lastName;
        Code = code;
        Nickname = nickname;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        DateOfDeath = dateOfDeath;
        PlaceOfBirth = placeOfBirth;
        PlaceOfDeath = placeOfDeath;
        Occupation = occupation;
        AvatarUrl = avatarUrl;
        Biography = biography;

        AddDomainEvent(new MemberUpdatedEvent(this));
    }

    public void UpdateBiography(string? biography)
    {
        Biography = biography;
        AddDomainEvent(new MemberBiographyUpdatedEvent(this));
    }

    public Relationship AddFatherRelationship(Guid fatherId)
    {
        var relationship = new Relationship(FamilyId, fatherId, Id, RelationshipType.Father);
        TargetRelationships.Add(relationship);
        return relationship;
    }

    public Relationship AddMotherRelationship(Guid motherId)
    {
        var relationship = new Relationship(FamilyId, motherId, Id, RelationshipType.Mother);
        TargetRelationships.Add(relationship);
        return relationship;
    }

    public (Relationship, Relationship) AddSpouseRelationship(Guid spouseId, backend.Domain.Enums.Gender currentMemberGender)
    {
        RelationshipType currentToSpouseType = currentMemberGender == backend.Domain.Enums.Gender.Male ? RelationshipType.Husband : RelationshipType.Wife;
        RelationshipType spouseToCurrentType = currentMemberGender == backend.Domain.Enums.Gender.Male ? RelationshipType.Wife : RelationshipType.Husband;

        var currentToSpouse = new Relationship(FamilyId, Id, spouseId, currentToSpouseType);
        var spouseToCurrent = new Relationship(FamilyId, spouseId, Id, spouseToCurrentType);

        SourceRelationships.Add(currentToSpouse);
        // We don't add spouseToCurrent to current member's relationships, as it belongs to the spouse's relationships.
        // It will be added to the context directly in the command handler.

        return (currentToSpouse, spouseToCurrent);
    }

    // Relationships
    public ICollection<Relationship> SourceRelationships { get; set; } = new List<Relationship>();
    public ICollection<Relationship> TargetRelationships { get; set; } = new List<Relationship>();
    public ICollection<EventMember> EventMembers { get; set; } = new List<EventMember>();

    public Member() { }

    public Member(string lastName, string firstName, string code, Guid familyId)
    {
        LastName = lastName;
        FirstName = firstName;
        Code = code;
        FamilyId = familyId;
    }

    public Member(string lastName, string firstName, string code, Guid familyId, string? nickname, string? gender, DateTime? dateOfBirth, DateTime? dateOfDeath, string? placeOfBirth, string? placeOfDeath, string? occupation, string? avatarUrl, string? biography)
        : this(lastName, firstName, code, familyId)
    {
        Nickname = nickname;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        DateOfDeath = dateOfDeath;
        PlaceOfBirth = placeOfBirth;
        PlaceOfDeath = placeOfDeath;
        Occupation = occupation;
        AvatarUrl = avatarUrl;
        Biography = biography;
    }

    public Member(Guid id, string lastName, string firstName, string code, Guid familyId, Family family)
        : this(lastName, firstName, code, familyId)
    {
        Id = id;
        Family = family;
    }

    public Member(Guid familyId)
    {
        FamilyId = familyId;
    }
}
