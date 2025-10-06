using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
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
            Type = RelationshipType.Father
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
        retrievedRelationship.Type.Should().Be(RelationshipType.Father);
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
        Context.Relationships.Add(new Relationship { SourceMemberId = father.Id, TargetMemberId = child1.Id, Type = RelationshipType.Father });
        Context.Relationships.Add(new Relationship { SourceMemberId = father.Id, TargetMemberId = child2.Id, Type = RelationshipType.Father });

        // Mother-Child relationships
        Context.Relationships.Add(new Relationship { SourceMemberId = mother.Id, TargetMemberId = child1.Id, Type = RelationshipType.Mother });
        Context.Relationships.Add(new Relationship { SourceMemberId = mother.Id, TargetMemberId = child2.Id, Type = RelationshipType.Mother });

        // Spouse relationship
        Context.Relationships.Add(new Relationship { SourceMemberId = father.Id, TargetMemberId = mother.Id, Type = RelationshipType.Husband });
        Context.Relationships.Add(new Relationship { SourceMemberId = mother.Id, TargetMemberId = father.Id, Type = RelationshipType.Wife });

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
        johnRelationships.Should().HaveCount(4); // 2 Father (to children) + 1 Husband (to wife) + 1 Wife (from wife)
        johnRelationships.Should().Contain(r => r.TargetMember.FirstName == "Alice" && r.Type == RelationshipType.Father);
        johnRelationships.Should().Contain(r => r.TargetMember.FirstName == "Bob" && r.Type == RelationshipType.Father);
        johnRelationships.Should().Contain(r => r.TargetMember.FirstName == "Jane" && r.Type == RelationshipType.Husband);
        johnRelationships.Should().Contain(r => r.SourceMember.FirstName == "Jane" && r.Type == RelationshipType.Wife);

        // Assert Alice's relationships
        aliceRelationships.Should().NotBeNull();
        aliceRelationships.Should().HaveCount(2); // 1 Father (from John) + 1 Mother (from Jane)
        aliceRelationships.Should().Contain(r => r.SourceMember.FirstName == "John" && r.Type == RelationshipType.Father);
        aliceRelationships.Should().Contain(r => r.SourceMember.FirstName == "Jane" && r.Type == RelationshipType.Mother);
    }
}
