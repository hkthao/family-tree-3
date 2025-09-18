using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using System;
using Xunit;

namespace backend.Domain.UnitTests.Entities;

public class RelationshipTests
{
    [Fact]
    public void Relationship_ShouldAllowSettingAndGettingProperties()
    {
        var familyId = Guid.NewGuid();
        var sourceMemberId = Guid.NewGuid();
        var targetMemberId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-10);
        var endDate = DateTime.UtcNow.AddDays(10);

        var relationship = new Relationship
        {
            FamilyId = familyId,
            SourceMemberId = sourceMemberId,
            TargetMemberId = targetMemberId,
            Type = RelationshipType.Child,
            StartDate = startDate,
            EndDate = endDate
        };

        relationship.FamilyId.Should().Be(familyId);
        relationship.SourceMemberId.Should().Be(sourceMemberId);
        relationship.TargetMemberId.Should().Be(targetMemberId);
        relationship.Type.Should().Be(RelationshipType.Child);
        relationship.StartDate.Should().Be(startDate);
        relationship.EndDate.Should().Be(endDate);
    }

    [Fact]
    public void Relationship_ShouldHandleNullDatesAndGuids()
    {
        var relationship = new Relationship
        {
            FamilyId = null,
            SourceMemberId = null,
            TargetMemberId = null,
            Type = RelationshipType.Parent,
            StartDate = null,
            EndDate = null
        };

        relationship.FamilyId.Should().BeNull();
        relationship.SourceMemberId.Should().BeNull();
        relationship.TargetMemberId.Should().BeNull();
        relationship.Type.Should().Be(RelationshipType.Parent);
        relationship.StartDate.Should().BeNull();
        relationship.EndDate.Should().BeNull();
    }
}
