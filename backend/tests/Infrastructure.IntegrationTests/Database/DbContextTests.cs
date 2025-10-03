using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using backend.Domain.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using backend.Domain.Enums;
namespace backend.Infrastructure.IntegrationTests.Database;

public class DbContextTests : TestBase
{
    [Fact]
    public async Task CanInsertAndGetMembers()
    {
        // Arrange
        var father = CreateMember("Father", "Test", "Male", new DateTime(1970, 1, 1));
        var child = CreateMember("Child", "Test", "Female", new DateTime(2000, 1, 1));

        Context.Members.Add(father);
        Context.Members.Add(child);
        await Context.SaveChangesAsync();

        // Act
        var retrievedFather = await Context.Members.FindAsync(father.Id);
        var retrievedChild = await Context.Members.FindAsync(child.Id);

        // Assert
        retrievedFather.Should().NotBeNull();
        retrievedFather!.FirstName.Should().Be("Father");
        retrievedChild.Should().NotBeNull();
        retrievedChild!.FirstName.Should().Be("Child");
    }

    [Fact]
    public async Task CanInsertAndGetRelationships()
    {
        // Arrange
        var father = CreateMember("Father", "Test", "Male", new DateTime(1970, 1, 1));
        var child = CreateMember("Child", "Test", "Female", new DateTime(2000, 1, 1));

        Context.Members.Add(father);
        Context.Members.Add(child);
        await Context.SaveChangesAsync();

        var relationship = new Relationship
        {
            SourceMemberId = father.Id,
            TargetMemberId = child.Id,
            Type = RelationshipType.ParentChild
        };

        Context.Relationships.Add(relationship);
        await Context.SaveChangesAsync();

        // Act
        var retrievedRelationship = await Context.Relationships
            .Include(r => r.SourceMember)
            .Include(r => r.TargetMember)
            .FirstOrDefaultAsync(r => r.Id == relationship.Id);

        // Assert
        retrievedRelationship.Should().NotBeNull();
        retrievedRelationship!.SourceMember.Should().NotBeNull();
        retrievedRelationship.SourceMember.FirstName.Should().Be("Father");
        retrievedRelationship.TargetMember.Should().NotBeNull();
        retrievedRelationship.TargetMember.FirstName.Should().Be("Child");
        retrievedRelationship.Type.Should().Be(RelationshipType.ParentChild);
    }

    [Fact]
    public async Task Member_RequiredFields_CannotBeNull()
    {
        // Arrange
        var member = new Member { FirstName = "Test", Gender = "Male", DateOfBirth = new DateTime(1990, 1, 1), FamilyId = DefaultFamily.Id }; // LastName is null

        Context.Members.Add(member);

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(async () => await Context.SaveChangesAsync());
    }

    [Fact]
    public async Task CanQueryFamilyRelationships()
    {
        // Arrange
        var father = CreateMember("John", "Doe", "Male", new DateTime(1970, 1, 1));
        var mother = CreateMember("Jane", "Doe", "Female", new DateTime(1972, 1, 1));
        var child1 = CreateMember("Alice", "Doe", "Female", new DateTime(1995, 1, 1));
        var child2 = CreateMember("Bob", "Doe", "Male", new DateTime(1998, 1, 1));

        Context.Members.AddRange(father, mother, child1, child2);
        await Context.SaveChangesAsync();

        // Father-Child relationships
        Context.Relationships.Add(new Relationship { SourceMemberId = father.Id, TargetMemberId = child1.Id, Type = RelationshipType.ParentChild });
        Context.Relationships.Add(new Relationship { SourceMemberId = father.Id, TargetMemberId = child2.Id, Type = RelationshipType.ParentChild });

        // Mother-Child relationships
        Context.Relationships.Add(new Relationship { SourceMemberId = mother.Id, TargetMemberId = child1.Id, Type = RelationshipType.ParentChild });
        Context.Relationships.Add(new Relationship { SourceMemberId = mother.Id, TargetMemberId = child2.Id, Type = RelationshipType.ParentChild });

        // Spouse relationship
        Context.Relationships.Add(new Relationship { SourceMemberId = father.Id, TargetMemberId = mother.Id, Type = RelationshipType.Spouse });
        Context.Relationships.Add(new Relationship { SourceMemberId = mother.Id, TargetMemberId = father.Id, Type = RelationshipType.Spouse });

        await Context.SaveChangesAsync();

        // Act
        var johnRelationships = await Context.Relationships
            .Include(r => r.SourceMember)
            .Include(r => r.TargetMember)
            .Where(r => r.SourceMemberId == father.Id || r.TargetMemberId == father.Id)
            .ToListAsync();

        var aliceRelationships = await Context.Relationships
            .Include(r => r.SourceMember)
            .Include(r => r.TargetMember)
            .Where(r => r.SourceMemberId == child1.Id || r.TargetMemberId == child1.Id)
            .ToListAsync();

        // Assert John's relationships
        johnRelationships.Should().NotBeNull();
        johnRelationships.Should().HaveCount(4); // 2 ParentChild (father as source) + 2 ParentChild (father as target) + 1 Spouse (father as source)
        johnRelationships.Should().Contain(r => r.TargetMember.FirstName == "Alice" && r.Type == RelationshipType.ParentChild);
        johnRelationships.Should().Contain(r => r.TargetMember.FirstName == "Bob" && r.Type == RelationshipType.ParentChild);
        johnRelationships.Should().Contain(r => r.TargetMember.FirstName == "Jane" && r.Type == RelationshipType.Spouse);
        johnRelationships.Should().Contain(r => r.SourceMember.FirstName == "Jane" && r.Type == RelationshipType.Spouse); // Mother is source for spouse relationship

        // Assert Alice's relationships
        aliceRelationships.Should().NotBeNull();
        aliceRelationships.Should().HaveCount(2); // 2 ParentChild (Alice as target)
        aliceRelationships.Should().Contain(r => r.SourceMember.FirstName == "John" && r.Type == RelationshipType.ParentChild);
        aliceRelationships.Should().Contain(r => r.SourceMember.FirstName == "Jane" && r.Type == RelationshipType.ParentChild);
    }
}
