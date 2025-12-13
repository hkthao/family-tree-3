using backend.Domain.Enums;
using backend.Domain.ValueObjects;

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

    /// <summary>
    /// Navigation property for events within this family.
    /// </summary>
    private readonly HashSet<Event> _events = new();
    public IReadOnlyCollection<Event> Events => _events;

    public PrivacyConfiguration? PrivacyConfiguration { get; private set; }

    public void AddFamilyUser(Guid userId, FamilyRole role)
    {
        if (_familyUsers.Any(fu => fu.UserId == userId && fu.Role == role))
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

    public void UpdateFamilyDetails(string name, string? description, string? address, string visibility, string code)
    {
        Name = name;
        Description = description;
        Address = address;
        Visibility = visibility;
        Code = code;
    }

    /// <summary>
    /// Cập nhật số liệu thống kê của gia đình (số thành viên, số sự kiện).
    /// </summary>
    /// <param name="memberCount">Tổng số thành viên.</param>
    /// <param name="eventCount">Tổng số sự kiện.</param>
    public void UpdateStats(int memberCount, int eventCount)
    {
        TotalMembers = memberCount;
        // Assuming there is a TotalEvents property or similar to update, if not, this part needs adjustment.
        // For now, let's assume eventCount might be used in a different stat or it's implicitly handled.
        // If there's a specific property for total events, it should be updated here.
        // Example: TotalEvents = eventCount;
    }

    public void UpdateAvatar(string? newAvatarUrl)
    {
        AvatarUrl = newAvatarUrl;
    }

    public void UpdateFamilyUsers(IEnumerable<FamilyUserUpdateInfo> newFamilyUsers)
    {
        var currentFamilyUsers = _familyUsers.ToList();
        var newFamilyUsersList = newFamilyUsers.ToList();

        // Remove users not in the new list
        foreach (var currentUser in currentFamilyUsers)
        {
            if (!newFamilyUsersList.Any(nf => nf.UserId == currentUser.UserId))
            {
                _familyUsers.Remove(currentUser);
            }
        }

        // Add or update users from the new list
        foreach (var newFamilyUser in newFamilyUsersList)
        {
            var existingUser = _familyUsers.FirstOrDefault(fu => fu.UserId == newFamilyUser.UserId);
            if (existingUser == null)
            {
                // Add new user
                _familyUsers.Add(new FamilyUser(Id, newFamilyUser.UserId, newFamilyUser.Role));
            }
            else
            {
                // Update existing user's role if changed
                if (existingUser.Role != newFamilyUser.Role)
                {
                    existingUser.Role = newFamilyUser.Role;
                }
            }
        }
    }

    public Member CreateMember(string lastName, string firstName, string code)
    {
        var member = new Member(lastName, firstName, code, Id);
        return member;
    }

    public Member AddMember(Member newMember, bool isRoot = false)
    {
        if (_members.Any(m => m.Code == newMember.Code))
        {
            throw new InvalidOperationException($"Member with code {newMember.Code} already exists in this family.");
        }

        if (isRoot)
        {
            var currentRoot = _members.FirstOrDefault(m => m.IsRoot);
            currentRoot?.UnsetAsRoot();
            newMember.SetAsRoot();
        }

        _members.Add(newMember);
        return newMember;
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

    public Relationship AddRelationship(Guid sourceMemberId, Guid targetMemberId, RelationshipType type, int? order)
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
        var relationship = new Relationship(Id, sourceMemberId, targetMemberId, type, order);
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

    /// <summary>
    /// Recalculates the total number of members in the family.
    /// </summary>
    public void RecalculateTotalMembers()
    {
        TotalMembers = _members.Count(m => !m.IsDeleted);
    }

    /// <summary>
    /// Recalculates the total number of generations in the family.
    /// </summary>
    public void RecalculateTotalGenerations()
    {
        // Build a graph representation of the family tree
        var graph = new Dictionary<Guid, List<Guid>>();
        var parents = new Dictionary<Guid, List<Guid>>();

        foreach (var member in _members)
        {
            graph[member.Id] = new List<Guid>();
            parents[member.Id] = new List<Guid>();
        }

        foreach (var rel in _relationships)
        {
            if (rel.Type == RelationshipType.Father || rel.Type == RelationshipType.Mother)
            {
                // Parent -> Child
                if (graph.ContainsKey(rel.SourceMemberId))
                {
                    graph[rel.SourceMemberId].Add(rel.TargetMemberId);
                }
                if (parents.ContainsKey(rel.TargetMemberId))
                {
                    parents[rel.TargetMemberId].Add(rel.SourceMemberId);
                }
            }
        }

        // Find all root members (members with no parents in the family)
        var rootMembers = _members.Where(m => !parents.ContainsKey(m.Id) || parents[m.Id].Count == 0).ToList();

        if (rootMembers.Count == 0 && _members.Count != 0) // If no explicit roots, consider members with no parents in the relationships as roots
        {
            rootMembers = _members.Where(m => !_relationships.Any(r => r.TargetMemberId == m.Id && (r.Type == RelationshipType.Father || r.Type == RelationshipType.Mother))).ToList();
        }

        if (rootMembers.Count == 0 && _members.Count != 0) // Fallback: if still no roots, just pick the first member
        {
            rootMembers.Add(_members.First());
        }

        int maxGenerations = 0;
        foreach (var root in rootMembers)
        {
            maxGenerations = Math.Max(maxGenerations, GetGenerations(root.Id, graph, new HashSet<Guid>()));
        }

        TotalGenerations = maxGenerations;
    }

    /// <summary>
    /// Recalculates all statistics for the family (e.g., total members, total generations).
    /// </summary>
    public void RecalculateStats()
    {
        RecalculateTotalMembers();
        RecalculateTotalGenerations();
    }

    private int GetGenerations(Guid memberId, Dictionary<Guid, List<Guid>> graph, HashSet<Guid> visited)
    {
        if (visited.Contains(memberId)) return 0; // Avoid infinite loops in case of circular relationships
        visited.Add(memberId);

        if (!graph.ContainsKey(memberId) || graph[memberId].Count == 0)
        {
            return 1; // Base case: leaf member is 1 generation
        }

        int maxChildGenerations = 0;
        foreach (var childId in graph[memberId])
        {
            maxChildGenerations = Math.Max(maxChildGenerations, GetGenerations(childId, graph, visited));
        }

        return 1 + maxChildGenerations;
    }

    // Public constructor for EF Core and seeding
    public Family() { }

    /// <summary>
    /// Factory method to create a new Family aggregate.
    /// </summary>
    public static Family Create(string name, string code, string? description, string? address, string visibility, Guid creatorUserId)
    {
        var family = new Family
        {
            Name = name,
            Code = code,
            Description = description,
            Address = address,
            Visibility = visibility,
            TotalMembers = 0, // Initial value
            TotalGenerations = 0 // Initial value
        };

        family.AddFamilyUser(creatorUserId, FamilyRole.Manager);

        return family;
    }
}
