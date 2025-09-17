using NUnit.Framework;
using Shouldly;
using backend.Domain.Entities;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace backend.Infrastructure.IntegrationTests.Families;

public class FamilyTests : IntegrationTestBase
{
    [Test]
    public async Task ShouldCreateFamily()
    {
        var dbContext = GetDbContext();
        var family = new Family { Name = "Test Family", Address = "123 Test St" };

        await dbContext.Families.InsertOneAsync(family);

        var createdFamily = await dbContext.Families.Find(f => f.Name == "Test Family").FirstOrDefaultAsync();
        createdFamily.ShouldNotBeNull();
        createdFamily.Name.ShouldBe("Test Family");
    }

    [Test]
    public async Task ShouldGetFamilyById()
    {
        var dbContext = GetDbContext();
        var family = new Family { Name = "Family to Get", Address = "456 Get Ave" };
        await dbContext.Families.InsertOneAsync(family);

        var retrievedFamily = await dbContext.Families.Find(f => f.Id == family.Id).FirstOrDefaultAsync();
        retrievedFamily.ShouldNotBeNull();
        retrievedFamily.Name.ShouldBe("Family to Get");
    }

    [Test]
    public async Task ShouldUpdateFamily()
    {
        var dbContext = GetDbContext();
        var family = new Family { Name = "Family to Update", Address = "789 Update Blvd" };
        await dbContext.Families.InsertOneAsync(family);

        family.Name = "Updated Family Name";
        var result = await dbContext.Families.ReplaceOneAsync(f => f.Id == family.Id, family);

        result.IsAcknowledged.ShouldBeTrue();
        result.ModifiedCount.ShouldBe(1);

        var updatedFamily = await dbContext.Families.Find(f => f.Id == family.Id).FirstOrDefaultAsync();
        updatedFamily.ShouldNotBeNull();
        updatedFamily.Name.ShouldBe("Updated Family Name");
    }

    [Test]
    public async Task ShouldDeleteFamily()
    {
        var dbContext = GetDbContext();
        var family = new Family { Name = "Family to Delete", Address = "101 Delete Rd" };
        await dbContext.Families.InsertOneAsync(family);

        var result = await dbContext.Families.DeleteOneAsync(f => f.Id == family.Id);

        result.IsAcknowledged.ShouldBeTrue();
        result.DeletedCount.ShouldBe(1);

        var deletedFamily = await dbContext.Families.Find(f => f.Id == family.Id).FirstOrDefaultAsync();
        deletedFamily.ShouldBeNull();
    }
}
