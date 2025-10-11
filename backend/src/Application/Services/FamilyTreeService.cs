using backend.Application.Common.Interfaces;
using backend.Domain.Enums;

namespace backend.Application.Services;

public class FamilyTreeService : IFamilyTreeService
{
    private readonly IApplicationDbContext _context;

    public FamilyTreeService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> CalculateTotalMembers(Guid familyId, CancellationToken cancellationToken = default)
    {
        return await _context.Members.CountAsync(m => m.FamilyId == familyId, cancellationToken);
    }

    public async Task<int> CalculateTotalGenerations(Guid familyId, CancellationToken cancellationToken = default)
    {
        var members = await _context.Members
            .Where(m => m.FamilyId == familyId)
            .Include(m => m.Relationships)
            .ToListAsync(cancellationToken);

        var relationships = await _context.Relationships
            .Where(r => members.Select(m => m.Id).Contains(r.SourceMemberId) || members.Select(m => m.Id).Contains(r.TargetMemberId))
            .ToListAsync(cancellationToken);

        // Build a graph representation of the family tree
        var graph = new Dictionary<Guid, List<Guid>>();
        var parents = new Dictionary<Guid, List<Guid>>();

        foreach (var member in members)
        {
            graph[member.Id] = [];
            parents[member.Id] = [];
        }

        foreach (var rel in relationships)
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
        var rootMembers = members.Where(m => !parents.ContainsKey(m.Id) || !parents[m.Id].Any()).ToList();

        if (!rootMembers.Any() && members.Any()) // If no explicit roots, consider members with no parents in the relationships as roots
        {
            rootMembers = members.Where(m => !relationships.Any(r => r.TargetMemberId == m.Id && (r.Type == RelationshipType.Father || r.Type == RelationshipType.Mother))).ToList();
        }

        if (!rootMembers.Any() && members.Any()) // Fallback: if still no roots, just pick the first member
        {
            rootMembers.Add(members.First());
        }

        int maxGenerations = 0;
        foreach (var root in rootMembers)
        {
            maxGenerations = Math.Max(maxGenerations, GetGenerations(root.Id, graph, []));
        }

        return maxGenerations;
    }

    private int GetGenerations(Guid memberId, Dictionary<Guid, List<Guid>> graph, HashSet<Guid> visited)
    {
        if (visited.Contains(memberId)) return 0; // Avoid infinite loops in case of circular relationships
        visited.Add(memberId);

        if (!graph.ContainsKey(memberId) || !graph[memberId].Any())
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

    public async Task UpdateFamilyStats(Guid familyId, CancellationToken cancellationToken = default)
    {
        var family = await _context.Families.FindAsync(familyId, cancellationToken);
        if (family == null) return; // Family not found

        family.TotalMembers = await CalculateTotalMembers(familyId, cancellationToken);
        family.TotalGenerations = await CalculateTotalGenerations(familyId, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
