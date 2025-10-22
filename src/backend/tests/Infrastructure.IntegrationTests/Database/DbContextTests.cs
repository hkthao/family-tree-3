using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace backend.Infrastructure.IntegrationTests.Database;

public class DbContextTests : IntegrationTestBase
{
    public DbContextTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }


    [Fact]
    public async Task CanInsertAndGetMembers()
    {
        // Arrange
        var father = new Member { Id = Guid.NewGuid(), FirstName = "Father", LastName = "Test", Gender = "Male", DateOfBirth = new DateTime(1970, 1, 1), Code = "FATHER001", FamilyId = Guid.NewGuid() };
        var child = new Member { Id = Guid.NewGuid(), FirstName = "Child", LastName = "Test", Gender = "Female", DateOfBirth = new DateTime(2000, 1, 1), Code = "CHILD001", FamilyId = father.FamilyId };

        _dbContext.Members.Add(father);
        _dbContext.Members.Add(child);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedFather = await _dbContext.Members.FindAsync(father.Id);
        var retrievedChild = await _dbContext.Members.FindAsync(child.Id);

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
        var father = new Member { Id = Guid.NewGuid(), FirstName = "Father", LastName = "Test", Gender = "Male", DateOfBirth = new DateTime(1970, 1, 1), Code = "FATHER002", FamilyId = Guid.NewGuid() };
        var child = new Member { Id = Guid.NewGuid(), FirstName = "Child", LastName = "Test", Gender = "Female", DateOfBirth = new DateTime(2000, 1, 1), Code = "CHILD002", FamilyId = father.FamilyId };

        _dbContext.Members.Add(father);
        _dbContext.Members.Add(child);
        await _dbContext.SaveChangesAsync();

        var relationship = new Relationship
        {
            SourceMemberId = father.Id,
            TargetMemberId = child.Id,
            Type = RelationshipType.Father
        };

        _dbContext.Relationships.Add(relationship);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedRelationship = await _dbContext.Relationships
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
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "FAM003" };
        _dbContext.Families.Add(family);
        await _dbContext.SaveChangesAsync();

        var member = new Member { FirstName = "Test", Gender = "Male", DateOfBirth = new DateTime(1990, 1, 1), FamilyId = familyId }; // Code is null

        _dbContext.Members.Add(member);

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(async () => await _dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task CanQueryFamilyRelationships()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var father = new Member { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Gender = "Male", DateOfBirth = new DateTime(1970, 1, 1), Code = "JOHN001", FamilyId = familyId };
        var mother = new Member { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", Gender = "Female", DateOfBirth = new DateTime(1972, 1, 1), Code = "JANE001", FamilyId = familyId };
        var child1 = new Member { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "Doe", Gender = "Female", DateOfBirth = new DateTime(1995, 1, 1), Code = "ALICE001", FamilyId = familyId };
        var child2 = new Member { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Doe", Gender = "Male", DateOfBirth = new DateTime(1998, 1, 1), Code = "BOB001", FamilyId = familyId };

        _dbContext.Members.AddRange(father, mother, child1, child2);
        await _dbContext.SaveChangesAsync();

        // Father-Child relationships
        _dbContext.Relationships.Add(new Relationship { SourceMemberId = father.Id, TargetMemberId = child1.Id, Type = RelationshipType.Father });
        _dbContext.Relationships.Add(new Relationship { SourceMemberId = father.Id, TargetMemberId = child2.Id, Type = RelationshipType.Father });

        // Mother-Child relationships
        _dbContext.Relationships.Add(new Relationship { SourceMemberId = mother.Id, TargetMemberId = child1.Id, Type = RelationshipType.Mother });
        _dbContext.Relationships.Add(new Relationship { SourceMemberId = mother.Id, TargetMemberId = child2.Id, Type = RelationshipType.Mother });

        // Spouse relationship
        _dbContext.Relationships.Add(new Relationship { SourceMemberId = father.Id, TargetMemberId = mother.Id, Type = RelationshipType.Husband });
        _dbContext.Relationships.Add(new Relationship { SourceMemberId = mother.Id, TargetMemberId = father.Id, Type = RelationshipType.Wife });

        await _dbContext.SaveChangesAsync();

        // Act
        var johnRelationships = await _dbContext.Relationships
            .Include(r => r.SourceMember)
            .Include(r => r.TargetMember)
            .Where(r => r.SourceMemberId == father.Id || r.TargetMemberId == father.Id)
            .ToListAsync();

        var aliceRelationships = await _dbContext.Relationships
            .Include(r => r.SourceMember)
            .Include(r => r.TargetMember)
            .Where(r => r.SourceMemberId == child1.Id || r.TargetMemberId == child1.Id)
            .ToListAsync();

        // Assert John's relationships
        johnRelationships.Should().NotBeNull();
        johnRelationships.Should().HaveCount(4); // 2 Father (to children) + 1 Husband (to wife) + 1 Wife (from wife)
        johnRelationships.Should().Contain(r => r.TargetMember.FirstName == "John" && r.Type == RelationshipType.Father);
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
