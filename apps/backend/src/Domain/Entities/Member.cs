using backend.Domain.Enums; // Add this for RelationshipType
using backend.Domain.Events.Members;

namespace backend.Domain.Entities;

public class Member : BaseAuditableEntity
{
    public string LastName { get; private set; } = null!; // Last name
    public string FirstName { get; private set; } = null!; // First name
    public string Code { get; private set; } = null!; // New property
    public string FullName => $"{LastName} {FirstName}"; // Full name (derived)
    public string? Nickname { get; private set; } // New
    public string? Gender { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public DateTime? DateOfDeath { get; private set; }
    public string? PlaceOfBirth { get; private set; }
    public string? PlaceOfDeath { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? Occupation { get; private set; } // New
    public string? AvatarUrl { get; private set; }
    public string? Biography { get; private set; } // New
    public bool IsDeceased { get; private set; } = false;
    public Guid FamilyId { get; private set; }
    public Family Family { get; private set; } = null!;
    public bool IsRoot { get; private set; } = false;
    public int? Order { get; private set; }

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

    public void Update(string firstName, string lastName, string code, string? nickname, string? gender, DateTime? dateOfBirth, DateTime? dateOfDeath, string? placeOfBirth, string? placeOfDeath, string? phone, string? email, string? address, string? occupation, string? avatarUrl, string? biography, int? order, bool isDeceased)
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

    public ICollection<MemberStory> MemberStories { get; private set; } = new List<MemberStory>();
    public ICollection<MemberFace> MemberFaces { get; private set; } = new List<MemberFace>(); // NEW

    public Member() { }

    public Member(string lastName, string firstName, string code, Guid familyId, bool isDeceased = false)
    {
        LastName = lastName;
        FirstName = firstName;
        Code = code;
        FamilyId = familyId;
        IsDeceased = isDeceased;
    }

    // New methods to manage MemberStory
    public void AddStory(MemberStory story)
    {
        if (story.MemberId != Id)
        {
            throw new InvalidOperationException("MemberStory must belong to this Member.");
        }
        MemberStories.Add(story);
    }

    public void RemoveStory(MemberStory story)
    {
        MemberStories.Remove(story);
    }

    // New method to manage MemberFace
    public void AddFace(MemberFace face)
    {
        if (face.MemberId != Id)
        {
            throw new InvalidOperationException("MemberFace must belong to this Member.");
        }
        MemberFaces.Add(face);
    }


    public Member(string lastName, string firstName, string code, Guid familyId, string? nickname, string? gender, DateTime? dateOfBirth, DateTime? dateOfDeath, string? placeOfBirth, string? placeOfDeath, string? phone, string? email, string? address, string? occupation, string? avatarUrl, string? biography, int? order, bool isDeceased)
        : this(lastName, firstName, code, familyId)
    {
        Nickname = nickname;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        DateOfDeath = dateOfDeath;
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
        : this(lastName, firstName, code, familyId, isDeceased)
    {
        Id = id;
        Family = family;
    }

    public Member(Guid familyId)
    {
        FamilyId = familyId;
    }
}
