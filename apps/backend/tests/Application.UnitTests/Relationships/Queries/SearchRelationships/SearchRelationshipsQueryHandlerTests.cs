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

        var family = Family.Create("Test Family", "TF", null, null, "Public", Guid.NewGuid());
        family.Id = familyId; // Set the ID to match the test's expectation
        var sourceMember1 = family.AddMember(new Member("Source1", "Member1", "SM1", family.Id) { Id = sourceMemberId1 });
        var targetMember1 = family.AddMember(new Member("Target1", "Member1", "TM1", family.Id) { Id = targetMemberId1 });
        var sourceMember2 = family.AddMember(new Member("Source2", "Member2", "SM2", family.Id) { Id = sourceMemberId2 });
        var targetMember2 = family.AddMember(new Member("Target2", "Member2", "TM2", family.Id) { Id = targetMemberId2 });
        _context.Families.Add(family);
        _context.Relationships.AddRange(
            new Relationship(familyId, sourceMemberId1, targetMemberId1, RelationshipType.Father, null),
            new Relationship(familyId, sourceMemberId2, targetMemberId2, RelationshipType.Mother, null)
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
            new Relationship(familyId, sourceMemberId, targetMemberId1, RelationshipType.Father, 1),
            new Relationship(familyId, sourceMemberId2, targetMemberId2, RelationshipType.Mother, 2)
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
            new Relationship(familyId, sourceMemberId1, targetMemberId, RelationshipType.Father, 1),
            new Relationship(familyId, sourceMemberId2, targetMemberId2, RelationshipType.Mother, 2)
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
            new Relationship(familyId, sourceMemberId1, targetMemberId1, RelationshipType.Father, 1),
            new Relationship(familyId, sourceMemberId2, targetMemberId2, RelationshipType.Mother, 2)
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

    [Fact]
    public async Task Handle_ShouldReturnNoRelationships_WhenUserDoesNotHaveAccessAndIsNotAdmin()
    {
        // Arrange
        var familyId1 = Guid.NewGuid();
        var familyId2 = Guid.NewGuid();
        var currentUserId = Guid.NewGuid(); // User not associated with either family

        _context.Families.Add(new Family { Id = familyId1, Name = "Test Family 1", Code = "TF1" });
        _context.Families.Add(new Family { Id = familyId2, Name = "Test Family 2", Code = "TF2" });

        _context.Relationships.AddRange(
            new Relationship(familyId1, Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Father, 1),
            new Relationship(familyId2, Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Mother, 2)
        );
        await _context.SaveChangesAsync();

        var handler = new SearchRelationshipsQueryHandler(_context, _mapper);
        var query = new SearchRelationshipsQuery { Page = 1, ItemsPerPage = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalItems.Should().Be(0);
    }

}
