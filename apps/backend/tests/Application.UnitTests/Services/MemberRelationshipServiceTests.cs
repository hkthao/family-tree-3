using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.Services;

public class MemberRelationshipServiceTests : TestBase
{
    private readonly MemberRelationshipService _memberRelationshipService;

    public MemberRelationshipServiceTests()
    {
        _memberRelationshipService = new MemberRelationshipService(_context);
    }

    [Fact]
    public async Task UpdateMemberRelationshipsAsync_ShouldClearExistingAndCreateNewBidirectionalRelationships()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var fatherId = Guid.NewGuid();
        var motherId = Guid.NewGuid();
        var husbandId = Guid.NewGuid();
        var wifeId = Guid.NewGuid();

        var member = new Member("Name", "Member", "MEM", familyId);
        member.Id = memberId;
        member.UpdateGender("Male");
        member.UpdateAvatar("member.jpg");

        var father = new Member("Name", "Father", "FTH", familyId);
        father.Id = fatherId;
        father.UpdateGender("Male");
        father.UpdateAvatar("father.jpg");

        var mother = new Member("Name", "Mother", "MTH", familyId);
        mother.Id = motherId;
        mother.UpdateGender("Female");
        mother.UpdateAvatar("mother.jpg");

        var husband = new Member("Name", "Husband", "HSB", familyId);
        husband.Id = husbandId;
        husband.UpdateGender("Male");
        husband.UpdateAvatar("husband.jpg");

        var wife = new Member("Name", "Wife", "WFE", familyId);
        wife.Id = wifeId;
        wife.UpdateGender("Female");
        wife.UpdateAvatar("wife.jpg");

        _context.Members.AddRange(member, father, mother, husband, wife);

        // Add some initial relationships for other members to ensure only relevant ones are cleared
        _context.Relationships.Add(new Relationship(familyId, Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Child));
        _context.Relationships.Add(new Relationship(familyId, memberId, Guid.NewGuid(), RelationshipType.Father)); // Example of an existing relationship for memberId

        await _context.SaveChangesAsync();

        // Act
        await _memberRelationshipService.UpdateMemberRelationshipsAsync(memberId, fatherId, motherId, husbandId, wifeId, CancellationToken.None);

        // Assert

        // Verify existing relationships for memberId are cleared and new ones created
        var relationshipsForMember = await _context.Relationships
            .Where(r => r.SourceMemberId == memberId || r.TargetMemberId == memberId)
            .ToListAsync();
        relationshipsForMember.Should().HaveCount(8); // 4 bidirectional relationships

        // Verify Father-Child relationship
        relationshipsForMember.Should().Contain(r => r.SourceMemberId == fatherId && r.TargetMemberId == memberId && r.Type == RelationshipType.Father);
        relationshipsForMember.Should().Contain(r => r.SourceMemberId == memberId && r.TargetMemberId == fatherId && r.Type == RelationshipType.Child);

        // Verify Mother-Child relationship
        relationshipsForMember.Should().Contain(r => r.SourceMemberId == motherId && r.TargetMemberId == memberId && r.Type == RelationshipType.Mother);
        relationshipsForMember.Should().Contain(r => r.SourceMemberId == memberId && r.TargetMemberId == motherId && r.Type == RelationshipType.Child);

        // Verify Husband-Wife relationship (member is target, husband is source)
        relationshipsForMember.Should().Contain(r => r.SourceMemberId == husbandId && r.TargetMemberId == memberId && r.Type == RelationshipType.Husband);
        relationshipsForMember.Should().Contain(r => r.SourceMemberId == memberId && r.TargetMemberId == husbandId && r.Type == RelationshipType.Wife);

        // Verify Wife-Husband relationship (member is target, wife is source)
        relationshipsForMember.Should().Contain(r => r.SourceMemberId == wifeId && r.TargetMemberId == memberId && r.Type == RelationshipType.Wife);
        relationshipsForMember.Should().Contain(r => r.SourceMemberId == memberId && r.TargetMemberId == wifeId && r.Type == RelationshipType.Husband);

        // Verify denormalized fields are updated
        var updatedMember = await _context.Members.FindAsync(memberId);
        updatedMember.Should().NotBeNull();

        updatedMember!.FatherId.Should().Be(fatherId);
        updatedMember.FatherFullName.Should().Be(father.FullName);
        updatedMember.FatherAvatarUrl.Should().Be(father.AvatarUrl);

        updatedMember.MotherId.Should().Be(motherId);
        updatedMember.MotherFullName.Should().Be(mother.FullName);
        updatedMember.MotherAvatarUrl.Should().Be(mother.AvatarUrl);

        updatedMember.HusbandId.Should().Be(husbandId);
        updatedMember.HusbandFullName.Should().Be(husband.FullName);
        updatedMember.HusbandAvatarUrl.Should().Be(husband.AvatarUrl);

        updatedMember.WifeId.Should().Be(wifeId);
        updatedMember.WifeFullName.Should().Be(wife.FullName);
        updatedMember.WifeAvatarUrl.Should().Be(wife.AvatarUrl);
    }
}
