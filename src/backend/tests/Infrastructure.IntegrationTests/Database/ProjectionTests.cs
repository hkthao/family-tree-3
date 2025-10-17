using Microsoft.EntityFrameworkCore;
using backend.Application.Members.Queries;

namespace backend.Infrastructure.IntegrationTests.Database;

public class ProjectionTests : TestBase
{
    [Fact]
    public async Task CanProjectMembersToMemberDto()
    {
        // Arrange
        var member1 = CreateMember("John", "Doe", "Male", new DateTime(1980, 1, 1));
        var member2 = CreateMember("Jane", "Smith", "Female", new DateTime(1985, 5, 10));

        Context.Members.Add(member1);
        Context.Members.Add(member2);
        await Context.SaveChangesAsync();

        // Act
        var memberDtos = await Context.Members
            .Select(m => new MemberDto
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName,
                Nickname = m.Nickname,
                Gender = m.Gender,
                DateOfBirth = m.DateOfBirth,
                DateOfDeath = m.DateOfDeath,
                PlaceOfBirth = m.PlaceOfBirth,
                PlaceOfDeath = m.PlaceOfDeath,
                Occupation = m.Occupation,
                AvatarUrl = m.AvatarUrl,
                Biography = m.Biography,
                FamilyId = m.FamilyId
            })
            .ToListAsync();

        // Assert
        memberDtos.Should().NotBeNull();
        memberDtos.Should().HaveCount(2);

        var dto1 = memberDtos.FirstOrDefault(dto => dto.Id == member1.Id);
        dto1.Should().NotBeNull();
        dto1!.FirstName.Should().Be("John");
        dto1.LastName.Should().Be("Doe");
        dto1.Nickname.Should().Be(member1.Nickname);
        dto1.Gender.Should().Be("Male");
        dto1.DateOfBirth.Should().Be(new DateTime(1980, 1, 1));
        dto1.DateOfDeath.Should().Be(member1.DateOfDeath);
        dto1.PlaceOfBirth.Should().Be(member1.PlaceOfBirth);
        dto1.PlaceOfDeath.Should().Be(member1.PlaceOfDeath);
        dto1.Occupation.Should().Be(member1.Occupation);
        dto1.AvatarUrl.Should().Be(member1.AvatarUrl);
        dto1.Biography.Should().Be(member1.Biography);
        dto1.FamilyId.Should().Be(member1.FamilyId);

        var dto2 = memberDtos.FirstOrDefault(dto => dto.Id == member2.Id);
        dto2.Should().NotBeNull();
        dto2!.FirstName.Should().Be("Jane");
        dto2.LastName.Should().Be("Smith");
        dto2.Nickname.Should().Be(member2.Nickname);
        dto2.Gender.Should().Be("Female");
        dto2.DateOfBirth.Should().Be(new DateTime(1985, 5, 10));
        dto2.DateOfDeath.Should().Be(member2.DateOfDeath);
        dto2.PlaceOfBirth.Should().Be(member2.PlaceOfBirth);
        dto2.PlaceOfDeath.Should().Be(member2.PlaceOfDeath);
        dto2.Occupation.Should().Be(member2.Occupation);
        dto2.AvatarUrl.Should().Be(member2.AvatarUrl);
        dto2.Biography.Should().Be(member2.Biography);
        dto2.FamilyId.Should().Be(member2.FamilyId);
    }
}
