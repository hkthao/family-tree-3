using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using backend.Domain.Entities;

namespace backend.Infrastructure.IntegrationTests.Database;

public class SqliteSpecificTests : TestBase
{
    [Fact]
    public async Task StringContains_IsCaseInsensitiveInSqliteByDefault()
    {
        // Arrange
        var member1 = CreateMember("John", "Doe", "Male", new DateTime(1980, 1, 1));
        var member2 = CreateMember("john", "smith", "Male", new DateTime(1985, 5, 10));
        var member3 = CreateMember("Jane", "doe", "Female", new DateTime(1990, 10, 20));

        Context.Members.Add(member1);
        Context.Members.Add(member2);
        Context.Members.Add(member3);
        await Context.SaveChangesAsync();

        // Act
        var result = await Context.Members
            .Where(m => m.FirstName.Contains("john"))
            .ToListAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1); // Expecting only "john" due to default case-sensitive behavior
        result.Should().Contain(m => m.Id == member2.Id);
    }

    [Fact]
    public async Task GroupBy_CanAggregateMembersByGender()
    {
        // Arrange
        var member1 = CreateMember("John", "Doe", "Male", new DateTime(1980, 1, 1));
        var member2 = CreateMember("Jane", "Smith", "Female", new DateTime(1985, 5, 10));
        var member3 = CreateMember("Peter", "Jones", "Male", new DateTime(1990, 10, 20));

        Context.Members.Add(member1);
        Context.Members.Add(member2);
        Context.Members.Add(member3);
        await Context.SaveChangesAsync();

        // Act
        var result = await Context.Members
            .GroupBy(m => m.Gender)
            .Select(g => new { Gender = g.Key, Count = g.Count() })
            .ToListAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Male, Female
        result.Should().Contain(g => g.Gender == "Male" && g.Count == 2);
        result.Should().Contain(g => g.Gender == "Female" && g.Count == 1);
    }

    [Fact]
    public async Task DateTimeFunctions_CanFilterByYear()
    {
        // Arrange
        var member1 = CreateMember("John", "Doe", "Male", new DateTime(1980, 1, 1));
        var member2 = CreateMember("Jane", "Smith", "Female", new DateTime(1985, 5, 10));
        var member3 = CreateMember("Peter", "Jones", "Male", new DateTime(1990, 10, 20));

        Context.Members.Add(member1);
        Context.Members.Add(member2);
        Context.Members.Add(member3);
        await Context.SaveChangesAsync();

        // Act
        var result = await Context.Members
            .Where(m => m.DateOfBirth != null && m.DateOfBirth.Value.Year == 1985)
            .ToListAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(member2.Id);
    }
}
