/*
using Xunit;
using FluentAssertions;
using backend.Domain.Entities;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace backend.Infrastructure.IntegrationTests.Families;

[Collection(nameof(IntegrationTestCollection))]
public class FamilyTests : IntegrationTestBase
{
    public FamilyTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ShouldCreateFamily()
    {
        var family = new Family { Name = "Test Family" };

        await _dbContext.Families.InsertOneAsync(family);

        var createdFamily = await _dbContext.Families.Find(f => f.Name == "Test Family").FirstOrDefaultAsync();
        createdFamily.Should().NotBeNull();
        createdFamily.Name.Should().Be("Test Family");
    }

    [Fact]
    public async Task ShouldGetFamilyById()
    {
        var family = new Family { Name = "Family to Get" };
        await _dbContext.Families.InsertOneAsync(family);

        var retrievedFamily = await _dbContext.Families.Find(f => f.Id == family.Id).FirstOrDefaultAsync();
        retrievedFamily.Should().NotBeNull();
        retrievedFamily.Name.Should().Be("Family to Get");
    }

    [Fact]
    public async Task ShouldUpdateFamily()
    {
        var family = new Family { Name = "Family to Update" };
        await _dbContext.Families.InsertOneAsync(family);

        family.Name = "Updated Family Name";
        var result = await _dbContext.Families.ReplaceOneAsync(f => f.Id == family.Id, family);

        result.IsAcknowledged.Should().BeTrue();
        result.ModifiedCount.Should().Be(1);

        var updatedFamily = await _dbContext.Families.Find(f => f.Id == family.Id).FirstOrDefaultAsync();
        updatedFamily.Should().NotBeNull();
        updatedFamily.Name.Should().Be("Updated Family Name");
    }

    [Fact]
    public async Task ShouldDeleteFamily()
    {
        var family = new Family { Name = "Family to Delete" };
        await _dbContext.Families.InsertOneAsync(family);

        var result = await _dbContext.Families.DeleteOneAsync(f => f.Id == family.Id);

        result.IsAcknowledged.Should().BeTrue();
        result.DeletedCount.Should().Be(1);

        var deletedFamily = await _dbContext.Families.Find(f => f.Id == family.Id).FirstOrDefaultAsync();
        deletedFamily.Should().BeNull();
    }
}
*/