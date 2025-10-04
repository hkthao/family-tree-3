using Microsoft.EntityFrameworkCore;
using backend.Domain.Entities;
using backend.Application.Common.Specifications;
using backend.Application.Members.Specifications;

namespace backend.Infrastructure.IntegrationTests.Database;

public class SpecificationTests : TestBase
{
    [Fact]
    public async Task CanFilterMembersByGender()
    {
        // Arrange
        var maleMember = CreateMember("John", "Doe", "Male", new DateTime(1980, 1, 1));
        var femaleMember = CreateMember("Jane", "Doe", "Female", new DateTime(1985, 5, 10));
        var otherMember = CreateMember("Pat", "Doe", "Other", new DateTime(1990, 10, 20));

        Context.Members.Add(maleMember);
        Context.Members.Add(femaleMember);
        Context.Members.Add(otherMember);
        await Context.SaveChangesAsync();

        // Act
        var spec = new MemberByGenderSpecification("Female");

        var query = SpecificationEvaluator<Member>.GetQuery(Context.Members.AsQueryable(), spec);
        var filteredMembers = await query.ToListAsync();

        // Assert
        filteredMembers.Should().NotBeNull();
        filteredMembers.Should().HaveCount(1);
        filteredMembers.First().Id.Should().Be(femaleMember.Id);
    }

    [Fact]
    public async Task CanFilterMembersByDateOfBirthRange()
    {
        // Arrange
        var member1 = CreateMember("John", "Doe", "Male", new DateTime(1970, 1, 1));
        var member2 = CreateMember("Jane", "Doe", "Female", new DateTime(1980, 5, 10));
        var member3 = CreateMember("Pat", "Doe", "Other", new DateTime(1990, 10, 20));

        Context.Members.Add(member1);
        Context.Members.Add(member2);
        Context.Members.Add(member3);
        await Context.SaveChangesAsync();

        // Act
        var startDate = new DateTime(1975, 1, 1);
        var endDate = new DateTime(1985, 12, 31);

        var spec = new MemberByDateOfBirthRangeSpecification(startDate, endDate);

        var query = SpecificationEvaluator<Member>.GetQuery(Context.Members.AsQueryable(), spec);
        var filteredMembers = await query.ToListAsync();

        // Assert
        filteredMembers.Should().NotBeNull();
        filteredMembers.Should().HaveCount(1);
        filteredMembers.First().Id.Should().Be(member2.Id);
    }

    [Fact]
    public async Task CanFilterMembersBySearchTermInFirstNameAndLastName()
    {
        // Arrange
        var member1 = CreateMember("John", "Doe", "Male", new DateTime(1970, 1, 1));
        var member2 = CreateMember("Jane", "Smith", "Female", new DateTime(1980, 5, 10));
        var member3 = CreateMember("Peter", "Jones", "Male", new DateTime(1990, 10, 20));

        Context.Members.Add(member1);
        Context.Members.Add(member2);
        Context.Members.Add(member3);
        await Context.SaveChangesAsync();

        // Act: Search for "Jo"
        var spec = new MemberFilterSpecification(searchTerm: "Jo", familyId: null, skip: 0, take: 100);
        var query = SpecificationEvaluator<Member>.GetQuery(Context.Members.AsQueryable(), spec);
        var filteredMembers = await query.ToListAsync();

        // Assert
        filteredMembers.Should().NotBeNull();
        filteredMembers.Should().HaveCount(2); // John Doe, Peter Jones
        filteredMembers.Should().Contain(m => m.Id == member1.Id);
        filteredMembers.Should().Contain(m => m.Id == member3.Id);
    }
}
