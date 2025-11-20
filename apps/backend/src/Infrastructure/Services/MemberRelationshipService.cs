using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services;

public class MemberRelationshipService : IMemberRelationshipService
{
    private readonly IApplicationDbContext _context;

    public MemberRelationshipService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task UpdateDenormalizedRelationshipFields(Member member, CancellationToken cancellationToken)
    {
        // Clear existing denormalized fields
        member.FatherId = null;
        member.FatherFullName = null;
        member.MotherId = null;
        member.MotherFullName = null;
        member.HusbandId = null;
        member.HusbandFullName = null;
        member.WifeId = null;
        member.WifeFullName = null;

        // Load all relationships involving this member
        var relationships = await _context.Relationships
            .Include(r => r.SourceMember)
            .Include(r => r.TargetMember)
            .Where(r => r.SourceMemberId == member.Id || r.TargetMemberId == member.Id)
            .ToListAsync(cancellationToken);

        // Find Father
        var fatherRelationship = relationships.FirstOrDefault(r =>
            r.TargetMemberId == member.Id && r.Type == RelationshipType.Father && r.SourceMember != null);
        if (fatherRelationship != null)
        {
            member.FatherId = fatherRelationship.SourceMemberId;
            member.FatherFullName = fatherRelationship.SourceMember?.FullName;
        }

        // Find Mother
        var motherRelationship = relationships.FirstOrDefault(r =>
            r.TargetMemberId == member.Id && r.Type == RelationshipType.Mother && r.SourceMember != null);
        if (motherRelationship != null)
        {
            member.MotherId = motherRelationship.SourceMemberId;
            member.MotherFullName = motherRelationship.SourceMember?.FullName;
        }

        // Find Husband (member is female, husband is SourceMember of Husband relationship where member is TargetMember)
        var husbandRelationship = relationships.FirstOrDefault(r =>
            r.TargetMemberId == member.Id && r.Type == RelationshipType.Husband && r.SourceMember != null);
        if (husbandRelationship != null)
        {
            member.HusbandId = husbandRelationship.SourceMemberId;
            member.HusbandFullName = husbandRelationship.SourceMember?.FullName;
        }

        // Find Wife (member is male, wife is SourceMember of Wife relationship where member is TargetMember)
        var wifeRelationship = relationships.FirstOrDefault(r =>
            r.TargetMemberId == member.Id && r.Type == RelationshipType.Wife && r.SourceMember != null);
        if (wifeRelationship != null)
        {
            member.WifeId = wifeRelationship.SourceMemberId;
            member.WifeFullName = wifeRelationship.SourceMember?.FullName;
        }
    }
}
