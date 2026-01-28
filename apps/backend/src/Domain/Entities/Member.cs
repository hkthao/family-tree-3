using backend.Domain.Enums; // Add this for RelationshipType
using backend.Domain.ValueObjects; // Add this

namespace backend.Domain.Entities;

public class Member : BaseAuditableEntity
{
    public string LastName { get; set; } = null!; // Last name
    public string FirstName { get; set; } = null!; // First name
    public string Code { get; set; } = null!; // New property
    public string FullName => $"{LastName} {FirstName}"; // Full name (derived)
    public string? Nickname { get; set; } // New
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfDeath { get; set; }
    public LunarDate? LunarDateOfDeath { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? PlaceOfDeath { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Occupation { get; set; } // New
    public string? AvatarUrl { get; set; }
    public string? Biography { get; set; } // New
    public bool IsDeceased { get; set; } = false;
    public Guid FamilyId { get; set; }
    public Family Family { get; set; } = null!;
    public bool IsRoot { get; set; } = false;
    public int? Order { get; set; }

    // Denormalized relationship properties
    public Guid? FatherId { get; set; }
    public string? FatherFullName { get; set; }
    public Guid? MotherId { get; set; }
    public string? MotherFullName { get; set; }
    public Guid? HusbandId { get; set; }
    public string? HusbandFullName { get; set; }
    public Guid? WifeId { get; set; }
    public string? WifeFullName { get; set; }

    public string? FatherAvatarUrl { get; set; }
    public string? MotherAvatarUrl { get; set; }
    public string? HusbandAvatarUrl { get; set; }
    public string? WifeAvatarUrl { get; set; }

    public void SetAsRoot()
    {
        IsRoot = true;
    }

    public void UnsetAsRoot()
    {
        IsRoot = false;
    }

    public void SetId(Guid id)
    {
        Id = id;
    }

    public void Update(string firstName, string lastName, string code, string? nickname, string? gender, DateTime? dateOfBirth, DateTime? dateOfDeath, LunarDate? lunarDateOfDeath, string? placeOfBirth, string? placeOfDeath, string? phone, string? email, string? address, string? occupation, string? avatarUrl, string? biography, int? order, bool isDeceased)
    {
        FirstName = firstName;
        LastName = lastName;
        Code = code;
        Nickname = nickname;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        DateOfDeath = dateOfDeath;
        LunarDateOfDeath = lunarDateOfDeath;
        PlaceOfBirth = placeOfBirth;
        PlaceOfDeath = placeOfDeath;
        Phone = phone;
        Email = email;
        Address = address;
        Occupation = occupation;
        AvatarUrl = avatarUrl;
        Biography = biography;
        Order = order;
        IsDeceased = isDeceased;
    }

    public void UpdateRelationShip(Guid? fatherId, string? fatherFullName, string? fatherAvatarUrl, Guid? motherId, string? motherFullName, string? motherAvatarUrl, Guid? husbandId, string? husbandFullName, string? husbandAvatarUrl, Guid? wifeId, string? wifeFullName, string? wifeAvatarUrl)
    {
        FatherId = fatherId;
        FatherFullName = fatherFullName;
        FatherAvatarUrl = fatherAvatarUrl;
        MotherId = motherId;
        MotherFullName = motherFullName;
        MotherAvatarUrl = motherAvatarUrl;
        HusbandId = husbandId;
        HusbandFullName = husbandFullName;
        HusbandAvatarUrl = husbandAvatarUrl;
        WifeId = wifeId;
        WifeFullName = wifeFullName;
        WifeAvatarUrl = wifeAvatarUrl;
    }

    public void UpdateBiography(string? biography)
    {
        Biography = biography;

    }

    public void UpdateAvatar(string? newAvatarUrl)
    {
        AvatarUrl = newAvatarUrl;
    }

    public void UpdateGender(string? gender)
    {
        Gender = gender;
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

    public Relationship AddHusbandRelationship(Guid husbandId)
    {
        var currentToSpouse = new Relationship(FamilyId, Id, husbandId, RelationshipType.Wife); // Current member (female) is wife of husbandId
        SourceRelationships.Add(currentToSpouse);
        return currentToSpouse;
    }

    public Relationship AddWifeRelationship(Guid wifeId)
    {
        var currentToSpouse = new Relationship(FamilyId, Id, wifeId, RelationshipType.Husband); // Current member (male) is husband of wifeId
        SourceRelationships.Add(currentToSpouse);
        return currentToSpouse;
    }

    // Relationships
    public ICollection<Relationship> SourceRelationships { get; set; } = new List<Relationship>();
    public ICollection<Relationship> TargetRelationships { get; set; } = new List<Relationship>();
    public ICollection<EventMember> EventMembers { get; set; } = new List<EventMember>();


    public ICollection<MemberFace> MemberFaces { get; set; } = new List<MemberFace>(); // NEW

    public Member() { }


    // New method to manage MemberFace
    public void AddFace(MemberFace face)
    {
        if (face.MemberId != Id)
        {
            throw new InvalidOperationException("MemberFace must belong to this Member.");
        }
        MemberFaces.Add(face);
    }


    public Member(string lastName, string firstName, string code, Guid familyId,
        string? nickname = null, string? gender = null, DateTime? dateOfBirth = null, DateTime? dateOfDeath = null,
        LunarDate? lunarDateOfDeath = null, // Replaced
        string? placeOfBirth = null, string? placeOfDeath = null, string? phone = null, string? email = null,
        string? address = null, string? occupation = null, string? avatarUrl = null, string? biography = null,
        int? order = null, bool isDeceased = false)
    {
        LastName = lastName;
        FirstName = firstName;
        Code = code;
        FamilyId = familyId;
        Nickname = nickname;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        DateOfDeath = dateOfDeath;
        LunarDateOfDeath = lunarDateOfDeath; // Assigned
        PlaceOfBirth = placeOfBirth;
        PlaceOfDeath = placeOfDeath;
        Phone = phone;
        Email = email;
        Address = address;
        Occupation = occupation;
        AvatarUrl = avatarUrl;
        Biography = biography;
        Order = order;
        IsDeceased = isDeceased;
    }

    public Member(Guid id, string lastName, string firstName, string code, Guid familyId, Family family, bool isDeceased = false)
        : this(lastName, firstName, code, familyId, isDeceased: isDeceased)
    {
        Id = id;
        Family = family;
    }

    public Member(Guid familyId)
    {
        FamilyId = familyId;
    }
}
