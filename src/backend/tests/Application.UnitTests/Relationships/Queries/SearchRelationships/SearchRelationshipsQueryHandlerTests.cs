using backend.Application.Relationships.Queries.SearchRelationships;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Queries.SearchRelationships;

public class SearchRelationshipsQueryHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnPaginatedRelationships()
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
            new Relationship(familyId, sourceMemberId1, targetMemberId1, RelationshipType.Father) { SourceMember = sourceMember1, TargetMember = targetMember1 },
            new Relationship(familyId, sourceMemberId2, targetMemberId2, RelationshipType.Mother) { SourceMember = sourceMember2, TargetMember = targetMember2 }
        );
        await _context.SaveChangesAsync();

        var handler = new SearchRelationshipsQueryHandler(_context, _mapper);
        var query = new SearchRelationshipsQuery { Page = 1, ItemsPerPage = 1 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.TotalItems.Should().Be(2);
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
            new Relationship(familyId, sourceMemberId, targetMemberId1, RelationshipType.Father) { SourceMember = sourceMember, TargetMember = targetMember1 },
            new Relationship(familyId, sourceMemberId2, targetMemberId2, RelationshipType.Mother) { SourceMember = sourceMember2, TargetMember = targetMember2 }
        );
        await _context.SaveChangesAsync();

        var handler = new SearchRelationshipsQueryHandler(_context, _mapper);
        var query = new SearchRelationshipsQuery { SourceMemberId = sourceMemberId };

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
            new Relationship(familyId, sourceMemberId1, targetMemberId, RelationshipType.Father) { SourceMember = sourceMember1, TargetMember = targetMember },
            new Relationship(familyId, sourceMemberId2, targetMemberId2, RelationshipType.Mother) { SourceMember = sourceMember2, TargetMember = targetMember2 }
        );
        await _context.SaveChangesAsync();

        var handler = new SearchRelationshipsQueryHandler(_context, _mapper);
        var query = new SearchRelationshipsQuery { TargetMemberId = targetMemberId };

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
            new Relationship(familyId, sourceMemberId1, targetMemberId1, RelationshipType.Father) { SourceMember = sourceMember1, TargetMember = targetMember1 },
            new Relationship(familyId, sourceMemberId2, targetMemberId2, RelationshipType.Mother) { SourceMember = sourceMember2, TargetMember = targetMember2 }
        );
        await _context.SaveChangesAsync();

        var handler = new SearchRelationshipsQueryHandler(_context, _mapper);
        var query = new SearchRelationshipsQuery { Type = "Father" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
    }
}