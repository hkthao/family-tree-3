using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries;
using backend.Application.Relationships.Queries;
using backend.Domain.Entities;
using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                .Where(m => m.FamilyId == familyId && m.IsRoot)
                .Select(m => m.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (currentRootMemberId == Guid.Empty)
        {
            return Result<FamilyTreeDataDto>.NotFound("Family does not have a root member or initial member not found.");
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

            // Fetch the current member
            var member = await _context.Members
                .Where(m => m.Id == currentMemberId && m.FamilyId == familyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (member != null)
            {
                allFetchedMembers.Add(member);

                // Fetch relationships involving this member
                var relatedRelationships = await _context.Relationships
                    .Where(r => r.FamilyId == familyId &&
                                (r.SourceMemberId == currentMemberId || r.TargetMemberId == currentMemberId))
                    .Include(r => r.SourceMember)
                    .Include(r => r.TargetMember)
                    .ToListAsync(cancellationToken);

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
                                membersQueue.Enqueue(rel.TargetMemberId);
                            }
                        }
                    }
                }
            }
        }
        
        var memberDtos = _mapper.Map<List<MemberDto>>(allFetchedMembers);
        var relationshipDtos = _mapper.Map<List<RelationshipDto>>(allFetchedRelationships);

        var familyTreeData = new FamilyTreeDataDto
        {
            Members = memberDtos,
            Relationships = relationshipDtos,
            RootMemberId = currentRootMemberId
        };

        return Result<FamilyTreeDataDto>.Success(familyTreeData);
    }
}
