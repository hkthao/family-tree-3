using System;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Domain.UnitTests.Entities;

public class MemberTests
{
    [Fact]
    public void ShouldCreateMember()
    {
        var familyId = Guid.NewGuid();
        var member = new Member
        {
            FirstName = "Test",
            LastName = "Member",
            Nickname = "Tester",
            DateOfBirth = new System.DateTime(1990, 1, 1),
            DateOfDeath = new System.DateTime(2020, 1, 1),
            PlaceOfBirth = "Test Place",
            PlaceOfDeath = "Test Death Place",
            Gender = "Male",
            AvatarUrl = "http://test.com/avatar.jpg",
            Occupation = "Developer",
            FamilyId = familyId,
            Biography = "Test Biography",
        };

        member.FirstName.Should().Be("Test");
        member.LastName.Should().Be("Member");
        member.FullName.Should().Be("Member Test");
        member.Nickname.Should().Be("Tester");
        member.DateOfBirth.Should().Be(new System.DateTime(1990, 1, 1));
        member.DateOfDeath.Should().Be(new System.DateTime(2020, 1, 1));
        member.PlaceOfBirth.Should().Be("Test Place");
        member.PlaceOfDeath.Should().Be("Test Death Place");
        member.Gender.Should().Be("Male");
        member.AvatarUrl.Should().Be("http://test.com/avatar.jpg");
        member.Occupation.Should().Be("Developer");
        member.FamilyId.Should().Be(familyId);
        member.Biography.Should().Be("Test Biography");
    }
}
