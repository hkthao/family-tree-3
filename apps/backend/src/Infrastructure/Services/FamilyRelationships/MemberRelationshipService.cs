using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Interfaces.Family;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services;

public class MemberRelationshipService(IApplicationDbContext context) : IMemberRelationshipService
{
    private readonly IApplicationDbContext _context = context;

    public async Task UpdateDenormalizedRelationshipFields(Member member, CancellationToken cancellationToken)
    {
        // Clear existing denormalized fields
        member.FatherId = null;
        member.FatherFullName = null;
        member.FatherAvatarUrl = null;
        member.MotherId = null;
        member.MotherFullName = null;
        member.MotherAvatarUrl = null;
        member.HusbandId = null;
        member.HusbandFullName = null;
        member.HusbandAvatarUrl = null;
        member.WifeId = null;
        member.WifeFullName = null;
        member.WifeAvatarUrl = null;

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
            member.FatherAvatarUrl = fatherRelationship.SourceMember?.AvatarUrl;
        }

        // Find Mother
        var motherRelationship = relationships.FirstOrDefault(r =>
            r.TargetMemberId == member.Id && r.Type == RelationshipType.Mother && r.SourceMember != null);
        if (motherRelationship != null)
        {
            member.MotherId = motherRelationship.SourceMemberId;
            member.MotherFullName = motherRelationship.SourceMember?.FullName;
            member.MotherAvatarUrl = motherRelationship.SourceMember?.AvatarUrl;
        }

        // Find Husband (member is female, husband is SourceMember of Husband relationship where member is TargetMember)
        var husbandRelationship = relationships.FirstOrDefault(r =>
            r.TargetMemberId == member.Id && r.Type == RelationshipType.Husband && r.SourceMember != null);
        if (husbandRelationship != null)
        {
            member.HusbandId = husbandRelationship.SourceMemberId;
            member.HusbandFullName = husbandRelationship.SourceMember?.FullName;
            member.HusbandAvatarUrl = husbandRelationship.SourceMember?.AvatarUrl;
        }

        // Find Wife (member is male, wife is SourceMember of Wife relationship where member is TargetMember)
        var wifeRelationship = relationships.FirstOrDefault(r =>
            r.TargetMemberId == member.Id && r.Type == RelationshipType.Wife && r.SourceMember != null);
        if (wifeRelationship != null)
        {
            member.WifeId = wifeRelationship.SourceMemberId;
            member.WifeFullName = wifeRelationship.SourceMember?.FullName;
            member.WifeAvatarUrl = wifeRelationship.SourceMember?.AvatarUrl;
        }
    }

    public async Task UpdateMemberRelationshipsAsync(Guid memberId, Guid? fatherId, Guid? motherId, Guid? husbandId, Guid? wifeId, CancellationToken cancellationToken)
    {
        // 1. Lấy thông tin thành viên hiện tại và FamilyId
        var currentMember = await _context.Members
            .Where(m => m.Id == memberId)
            .Select(m => new { Member = m, m.FamilyId })
            .FirstOrDefaultAsync(cancellationToken);

        if (currentMember == null)
        {
            return; // Member not found
        }
        var familyId = currentMember.FamilyId;

        // 2. Xóa tất cả các mối quan hệ hiện có liên quan đến thành viên này
        var existingRelationships = await _context.Relationships
            .Where(r => r.SourceMemberId == memberId || r.TargetMemberId == memberId)
            .ToListAsync(cancellationToken);

        _context.Relationships.RemoveRange(existingRelationships);
        await _context.SaveChangesAsync(cancellationToken);

        // 3. Tạo lại các mối quan hệ hai chiều
        var relationshipsToCreate = new List<Relationship>();

        // Helper to create bidirectional relationships
        void CreateBidirectionalRelationship(Guid sourceId, Guid targetId, RelationshipType sourceToTarget, RelationshipType targetToSource)
        {
            relationshipsToCreate.Add(new Relationship(familyId, sourceId, targetId, sourceToTarget));
            relationshipsToCreate.Add(new Relationship(familyId, targetId, sourceId, targetToSource));
        }

        if (fatherId.HasValue)
        {
            CreateBidirectionalRelationship(fatherId.Value, memberId, RelationshipType.Father, RelationshipType.Child);
        }
        if (motherId.HasValue)
        {
            CreateBidirectionalRelationship(motherId.Value, memberId, RelationshipType.Mother, RelationshipType.Child);
        }
        if (husbandId.HasValue)
        {
            CreateBidirectionalRelationship(husbandId.Value, memberId, RelationshipType.Husband, RelationshipType.Wife);
        }
        if (wifeId.HasValue)
        {
            CreateBidirectionalRelationship(wifeId.Value, memberId, RelationshipType.Wife, RelationshipType.Husband);
        }

        // cac moi quan he lien quan
        var childs = await _context.Members.Where(e => e.FatherId == memberId || e.MotherId == memberId).AsNoTracking().ToListAsync();
        foreach (var child in childs)
        {
            var relationshipType = child.FatherId == memberId ? RelationshipType.Father : RelationshipType.Mother;
            CreateBidirectionalRelationship(memberId, child.Id, relationshipType, RelationshipType.Child);
        }

        _context.Relationships.AddRange(relationshipsToCreate);
        await _context.SaveChangesAsync(cancellationToken);

        // 4. Cập nhật các trường FullName và AvatarUrl của thành viên
        currentMember.Member.FatherId = fatherId;
        currentMember.Member.MotherId = motherId;
        currentMember.Member.HusbandId = husbandId;
        currentMember.Member.WifeId = wifeId;

        if (fatherId.HasValue)
        {
            var father = await _context.Members.FindAsync(fatherId.Value);
            currentMember.Member.FatherFullName = father?.FullName;
            currentMember.Member.FatherAvatarUrl = father?.AvatarUrl;
        }
        else
        {
            currentMember.Member.FatherFullName = null;
            currentMember.Member.FatherAvatarUrl = null;
        }

        if (motherId.HasValue)
        {
            var mother = await _context.Members.FindAsync(motherId.Value);
            currentMember.Member.MotherFullName = mother?.FullName;
            currentMember.Member.MotherAvatarUrl = mother?.AvatarUrl;
        }
        else
        {
            currentMember.Member.MotherFullName = null;
            currentMember.Member.MotherAvatarUrl = null;
        }

        if (husbandId.HasValue)
        {
            var husband = await _context.Members.FindAsync(husbandId.Value);
            currentMember.Member.HusbandFullName = husband?.FullName;
            currentMember.Member.HusbandAvatarUrl = husband?.AvatarUrl;
        }
        else
        {
            currentMember.Member.HusbandFullName = null;
            currentMember.Member.HusbandAvatarUrl = null;
        }

        if (wifeId.HasValue)
        {
            var wife = await _context.Members.FindAsync(wifeId.Value);
            currentMember.Member.WifeFullName = wife?.FullName;
            currentMember.Member.WifeAvatarUrl = wife?.AvatarUrl;
        }
        else
        {
            currentMember.Member.WifeFullName = null;
            currentMember.Member.WifeAvatarUrl = null;
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}
