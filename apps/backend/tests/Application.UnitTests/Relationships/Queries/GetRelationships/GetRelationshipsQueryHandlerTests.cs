
using backend.Application.Relationships.Queries.GetRelationships;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Queries.GetRelationships;

public class GetRelationshipsQueryHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnPaginatedListOfRelationships()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var sourceMemberId1 = Guid.NewGuid();
        var targetMemberId1 = Guid.NewGuid();
        var sourceMemberId2 = Guid.NewGuid();
        var targetMemberId2 = Guid.NewGuid();

        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF" });
        var sourceMember1 = new Member("Source1", "Member1", "SM1", familyId) { Id = sourceMemberId1 };
        var targetMember1 = new Member("Target1", "Member1", "TM1", familyId) { Id = targetMemberId1 };
        var sourceMember2 = new Member("Source2", "Member2", "SM2", familyId) { Id = sourceMemberId2 };
        var targetMember2 = new Member("Target2", "Member2", "TM2", familyId) { Id = targetMemberId2 };
        _context.Members.AddRange(sourceMember1, targetMember1, sourceMember2, targetMember2);
        _context.Relationships.AddRange(
            new Relationship(familyId, sourceMemberId1, targetMemberId1, RelationshipType.Father, null),
            new Relationship(familyId, sourceMemberId2, targetMemberId2, RelationshipType.Mother, null)
        );
        await _context.SaveChangesAsync();

        // Act
        var handler = new GetRelationshipsQueryHandler(_context, _mapper);
        var query = new GetRelationshipsQuery();
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalItems.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterByFamilyId()
    {
        // Arrange
        var familyId1 = Guid.NewGuid();
        var familyId2 = Guid.NewGuid();
        var sourceMemberId1 = Guid.NewGuid();
        var targetMemberId1 = Guid.NewGuid();
        var sourceMemberId2 = Guid.NewGuid();
        var targetMemberId2 = Guid.NewGuid();

        _context.Families.AddRange(
            new Family { Id = familyId1, Name = "Family 1", Code = "F1" },
            new Family { Id = familyId2, Name = "Family 2", Code = "F2" }
        );
        var sourceMember1 = new Member("Source1", "Member1", "SM1", familyId1) { Id = sourceMemberId1 };
        var targetMember1 = new Member("Target1", "Member1", "TM1", familyId1) { Id = targetMemberId1 };
        var sourceMember2 = new Member("Source2", "Member2", "SM2", familyId2) { Id = sourceMemberId2 };
        var targetMember2 = new Member("Target2", "Member2", "TM2", familyId2) { Id = targetMemberId2 };
        _context.Members.AddRange(sourceMember1, targetMember1, sourceMember2, targetMember2);
        _context.Relationships.AddRange(
            new Relationship(familyId1, sourceMemberId1, targetMemberId1, RelationshipType.Father, 1),
            new Relationship(familyId2, sourceMemberId2, targetMemberId2, RelationshipType.Mother, 1)
        );
        await _context.SaveChangesAsync();

        var handler = new GetRelationshipsQueryHandler(_context, _mapper);
        var query = new GetRelationshipsQuery { FamilyId = familyId1 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldFilterBySourceMemberId()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var sourceMemberId = Guid.NewGuid();
        var targetMemberId1 = Guid.NewGuid();
        var sourceMemberId2 = Guid.NewGuid();
        var targetMemberId2 = Guid.NewGuid();

        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF" });
        var sourceMember = new Member("Source1", "Member1", "SM1", familyId) { Id = sourceMemberId };
        var targetMember1 = new Member("Target1", "Member1", "TM1", familyId) { Id = targetMemberId1 };
        var sourceMember2 = new Member("Source2", "Member2", "SM2", familyId) { Id = sourceMemberId2 };
        var targetMember2 = new Member("Target2", "Member2", "TM2", familyId) { Id = targetMemberId2 };
        _context.Members.AddRange(sourceMember, targetMember1, sourceMember2, targetMember2);
        _context.Relationships.AddRange(
            new Relationship(familyId, sourceMemberId, targetMemberId1, RelationshipType.Father, 1),
            new Relationship(familyId, sourceMemberId2, targetMemberId2, RelationshipType.Mother, 1)
        );
        await _context.SaveChangesAsync();

        var handler = new GetRelationshipsQueryHandler(_context, _mapper);
        var query = new GetRelationshipsQuery { SourceMemberId = sourceMemberId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldFilterByTargetMemberId()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var sourceMemberId1 = Guid.NewGuid();
        var targetMemberId = Guid.NewGuid();
        var sourceMemberId2 = Guid.NewGuid();
        var targetMemberId2 = Guid.NewGuid();

        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF" });
        var sourceMember1 = new Member("Source1", "Member1", "SM1", familyId) { Id = sourceMemberId1 };
        var targetMember = new Member("Target1", "Member1", "TM1", familyId) { Id = targetMemberId };
        var sourceMember2 = new Member("Source2", "Member2", "SM2", familyId) { Id = sourceMemberId2 };
        var targetMember2 = new Member("Target2", "Member2", "TM2", familyId) { Id = targetMemberId2 };
        _context.Members.AddRange(sourceMember1, targetMember, sourceMember2, targetMember2);
        _context.Relationships.AddRange(
            new Relationship(familyId, sourceMemberId1, targetMemberId, RelationshipType.Father, 1),
            new Relationship(familyId, sourceMemberId2, targetMemberId2, RelationshipType.Mother, 1)
        );
        await _context.SaveChangesAsync();

        var handler = new GetRelationshipsQueryHandler(_context, _mapper);
        var query = new GetRelationshipsQuery { TargetMemberId = targetMemberId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldFilterByType()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var sourceMemberId1 = Guid.NewGuid();
        var targetMemberId1 = Guid.NewGuid();
        var sourceMemberId2 = Guid.NewGuid();
        var targetMemberId2 = Guid.NewGuid();

        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF" });
        var sourceMember1 = new Member("Source1", "Member1", "SM1", familyId) { Id = sourceMemberId1 };
        var targetMember1 = new Member("Target1", "Member1", "TM1", familyId) { Id = targetMemberId1 };
        var sourceMember2 = new Member("Source2", "Member2", "SM2", familyId) { Id = sourceMemberId2 };
        var targetMember2 = new Member("Target2", "Member2", "TM2", familyId) { Id = targetMemberId2 };
        _context.Members.AddRange(sourceMember1, targetMember1, sourceMember2, targetMember2);
        _context.Relationships.AddRange(
            new Relationship(familyId, sourceMemberId1, targetMemberId1, RelationshipType.Father, 1),
            new Relationship(familyId, sourceMemberId2, targetMemberId2, RelationshipType.Mother, 1)
        );
        await _context.SaveChangesAsync();

        var handler = new GetRelationshipsQueryHandler(_context, _mapper);
        var query = new GetRelationshipsQuery { Type = "Father" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
    }
}
