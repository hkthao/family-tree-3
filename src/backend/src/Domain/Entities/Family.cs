using backend.Domain.Enums;

namespace backend.Domain.Entities;

/// <summary>
/// Represents a Family Aggregate Root.
/// Handles member, relationship, and user associations within a family to ensure consistency.
/// Nó encapsulates các thực thể con như FamilyUser, Member và Relationship.
/// Những thay đổi đối với các thực thể con này chỉ nên được thực hiện thông qua Aggregate Root Family.
/// </summary>
public class Family : BaseAuditableEntity, IAggregateRoot
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }

    /// <summary>
    /// Dia chi
    /// </summary>
    public string? Address { get; set; }

    public string? AvatarUrl { get; set; }
    public string Visibility { get; set; } = "Private"; // e.g., Private, Public
    public int TotalMembers { get; set; }
    public int TotalGenerations { get; set; }

    /// <summary>
    /// Navigation property for users associated with this family.
    /// </summary>
    private readonly HashSet<FamilyUser> _familyUsers = new();
    public IReadOnlyCollection<FamilyUser> FamilyUsers => _familyUsers;

    /// <summary>
    /// Navigation property for members within this family.
    /// </summary>
    private readonly HashSet<Member> _members = new();
    public IReadOnlyCollection<Member> Members => _members;

    /// <summary>
    /// Navigation property for relationships within this family.
    /// </summary>
    private readonly HashSet<Relationship> _relationships = new();
    public IReadOnlyCollection<Relationship> Relationships => _relationships;

    public void AddFamilyUser(Guid userId, FamilyRole role)
    {
        if (_familyUsers.Any(fu => fu.UserId == userId))
        {
            throw new InvalidOperationException($"User with ID {userId} is already part of this family.");
        }
        _familyUsers.Add(new FamilyUser(Id, userId, role));
    }

    public void RemoveFamilyUser(Guid userId)
    {
        var familyUser = _familyUsers.FirstOrDefault(fu => fu.UserId == userId);
        if (familyUser != null)
        {
            _familyUsers.Remove(familyUser);
        }
    }

    public Member AddMember(string lastName, string firstName, string code)
    {
        if (_members.Any(m => m.Code == code))
        {
            throw new InvalidOperationException($"Member with code {code} already exists in this family.");
        }
        var member = new Member(lastName, firstName, code, Id);
        _members.Add(member);
        return member;
    }

    public void RemoveMember(Guid memberId)
    {
        var member = _members.FirstOrDefault(m => m.Id == memberId);
        if (member != null)
        {
            _members.Remove(member);
            // Also remove any relationships involving this member
            var relationshipsToRemove = _relationships.Where(r => r.SourceMemberId == memberId || r.TargetMemberId == memberId).ToList();
            foreach (var relationship in relationshipsToRemove)
            {
                _relationships.Remove(relationship);
            }
        }
    }

    public Relationship AddRelationship(Guid sourceMemberId, Guid targetMemberId, RelationshipType type)
    {
        if (!_members.Any(m => m.Id == sourceMemberId))
        {
            throw new InvalidOperationException($"Source member with ID {sourceMemberId} not found in this family.");
        }
        if (!_members.Any(m => m.Id == targetMemberId))
        {
            throw new InvalidOperationException($"Target member with ID {targetMemberId} not found in this family.");
        }
        // Add validation for duplicate relationships if needed
        var relationship = new Relationship(Id, sourceMemberId, targetMemberId, type);
        _relationships.Add(relationship);
        return relationship;
    }

    public void RemoveRelationship(Guid relationshipId)
    {
        var relationship = _relationships.FirstOrDefault(r => r.Id == relationshipId);
        if (relationship != null)
        {
            _relationships.Remove(relationship);
        }
    }
}
