using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries;
using backend.Application.Relationships.Queries;
using backend.Domain.Entities;

namespace backend.Application.Families.Queries.GetFamilyTreeData;

public class GetFamilyTreeDataQueryHandler : IRequestHandler<GetFamilyTreeDataQuery, Result<FamilyTreeDataDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private const int MAX_LIMIT = 50; // Hardcoded limit as requested

    public GetFamilyTreeDataQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<FamilyTreeDataDto>> Handle(GetFamilyTreeDataQuery request, CancellationToken cancellationToken)
    {
        var familyId = request.FamilyId;
        var initialMemberId = request.InitialMemberId;
        
        // 1. Determine the root member if initialMemberId is null
        Guid currentRootMemberId;
        if (initialMemberId.HasValue && initialMemberId.Value != Guid.Empty)
        {
            currentRootMemberId = initialMemberId.Value;
        }
        else
        {
            currentRootMemberId = await _context.Members
                .AsNoTracking() // Add AsNoTracking for read-only query
                .Where(m => m.FamilyId == familyId && m.IsRoot)
                .Select(m => m.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (currentRootMemberId == Guid.Empty)
        {
            return Result<FamilyTreeDataDto>.Success(new FamilyTreeDataDto
            {
                Members = new List<MemberDto>(),
                Relationships = new List<RelationshipDto>(),
                RootMemberId = null
            });
        }

        var membersQueue = new Queue<Guid>();
        membersQueue.Enqueue(currentRootMemberId);

        var allFetchedMembers = new List<Member>();
        var allFetchedRelationships = new List<Relationship>();
        var visitedMembers = new HashSet<Guid>();
        var visitedRelationships = new HashSet<Guid>();
        
        // BFS traversal to fetch connected members and relationships, with a maximum limit
        while (membersQueue.Any() && allFetchedMembers.Count < MAX_LIMIT) // Apply MAX_LIMIT here
        {
            var currentMemberId = membersQueue.Dequeue();

            if (!visitedMembers.Add(currentMemberId))
            {
                continue;
            }

            // Fetch the current member (full entity needed for MemberDto mapping)
            var member = await _context.Members
                .AsNoTracking() // Add AsNoTracking for read-only query
                .Where(m => m.Id == currentMemberId && m.FamilyId == familyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (member != null)
            {
                allFetchedMembers.Add(member);

                // Fetch relationships involving this member without including related members
                var relatedRelationships = await _context.Relationships
                    .AsNoTracking() // Add AsNoTracking for read-only query
                    .Where(r => r.FamilyId == familyId && (r.SourceMemberId == currentMemberId || r.TargetMemberId == currentMemberId))
                    .ToListAsync(cancellationToken); // Removed .Include(r => r.SourceMember).Include(r => r.TargetMember)

                foreach (var rel in relatedRelationships)
                {
                    if (visitedRelationships.Add(rel.Id))
                    {
                        allFetchedRelationships.Add(rel);

                        // Add connected members to queue if not visited and within limit
                        if (allFetchedMembers.Count < MAX_LIMIT) // Check limit before enqueuing
                        {
                            if (rel.SourceMemberId != currentMemberId && !visitedMembers.Contains(rel.SourceMemberId))
                            {
                                membersQueue.Enqueue(rel.SourceMemberId);
                            }
                            if (rel.TargetMemberId != currentMemberId && !visitedMembers.Contains(rel.TargetMemberId))
                            {
                            // Important: Ensure we don't exceed MAX_LIMIT when enqueuing
                                if (allFetchedMembers.Count < MAX_LIMIT)
                                {
                                    membersQueue.Enqueue(rel.TargetMemberId);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Collect all unique member IDs involved in fetched relationships for batch lookup
        var allRelatedMemberIds = new HashSet<Guid>();
        foreach (var rel in allFetchedRelationships)
        {
            allRelatedMemberIds.Add(rel.SourceMemberId);
            allRelatedMemberIds.Add(rel.TargetMemberId);
        }
        // Also include the currentRootMemberId and any members fetched explicitly if not already included
        allRelatedMemberIds.UnionWith(allFetchedMembers.Select(m => m.Id));


        // Batch fetch details for all related members (to populate RelationshipMemberDto)
        var relatedMemberDetails = await _context.Members
            .AsNoTracking()
            .Where(m => allRelatedMemberIds.Contains(m.Id))
            .Select(m => new RelationshipMemberDto // Project directly to RelationshipMemberDto to get only needed fields
            {
                Id = m.Id,
                FullName = m.LastName + " " + m.FirstName, // Construct FullName here
                IsRoot = m.IsRoot,
                AvatarUrl = m.AvatarUrl,
                DateOfBirth = m.DateOfBirth
            })
            .ToDictionaryAsync(m => m.Id, cancellationToken);
        
        // Map relationships and populate SourceMember/TargetMember
        var relationshipDtos = new List<RelationshipDto>();
        foreach (var rel in allFetchedRelationships)
        {
            var relDto = _mapper.Map<RelationshipDto>(rel);
            
            if (relatedMemberDetails.TryGetValue(rel.SourceMemberId, out var sourceMemberDto))
            {
                relDto.SourceMember = sourceMemberDto;
            }
            if (relatedMemberDetails.TryGetValue(rel.TargetMemberId, out var targetMemberDto))
            {
                relDto.TargetMember = targetMemberDto;
            }
            relationshipDtos.Add(relDto);
        }

        var memberDtos = _mapper.Map<List<MemberDto>>(allFetchedMembers);

        var familyTreeData = new FamilyTreeDataDto
        {
            Members = memberDtos,
            Relationships = relationshipDtos,
            RootMemberId = currentRootMemberId
        };

        return Result<FamilyTreeDataDto>.Success(familyTreeData);
    }
}
