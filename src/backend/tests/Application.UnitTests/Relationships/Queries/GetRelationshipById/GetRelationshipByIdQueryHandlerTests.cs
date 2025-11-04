using backend.Application.Relationships.Queries.GetRelationshipById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Queries.GetRelationshipById;

public class GetRelationshipByIdQueryHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnRelationship_WhenRelationshipExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var sourceMemberId = Guid.NewGuid();
        var targetMemberId = Guid.NewGuid();
        var relationshipId = Guid.NewGuid();

        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF" };
        var sourceMember = new Member("Source", "Member", "SM", familyId) { Id = sourceMemberId };
        var targetMember = new Member("Target", "Member", "TM", familyId) { Id = targetMemberId };
        var relationship = new Relationship(familyId, sourceMemberId, targetMemberId, RelationshipType.Father, 1) { Id = relationshipId };
        _context.Families.Add(family);
        _context.Members.AddRange(sourceMember, targetMember);
        _context.Relationships.Add(relationship);
        await _context.SaveChangesAsync();

        var handler = new GetRelationshipByIdQueryHandler(_context, _mapper);
        var query = new GetRelationshipByIdQuery(relationshipId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(relationshipId);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRelationshipDoesNotExist()
    {
        // Arrange
        var handler = new GetRelationshipByIdQueryHandler(_context, _mapper);
        var query = new GetRelationshipByIdQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
