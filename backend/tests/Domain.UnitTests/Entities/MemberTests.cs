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
            FullName = "Test Member",
            DateOfBirth = new System.DateTime(1990, 1, 1),
            DateOfDeath = new System.DateTime(2020, 1, 1),
            PlaceOfBirth = "Test Place",
            Gender = "Male",
            AvatarUrl = "http://test.com/avatar.jpg",
            Phone = "1234567890",
            Email = "test@test.com",
            Generation = 1,
            FamilyId = familyId,
            Biography = "Test Biography",
            Metadata = "Test Metadata"
        };

        member.FullName.Should().Be("Test Member");
        member.DateOfBirth.Should().Be(new System.DateTime(1990, 1, 1));
        member.DateOfDeath.Should().Be(new System.DateTime(2020, 1, 1));
        member.PlaceOfBirth.Should().Be("Test Place");
        member.Gender.Should().Be("Male");
        member.AvatarUrl.Should().Be("http://test.com/avatar.jpg");
        member.Phone.Should().Be("1234567890");
        member.Email.Should().Be("test@test.com");
        member.Generation.Should().Be(1);
        member.FamilyId.Should().Be(familyId);
        member.Biography.Should().Be("Test Biography");
        member.Metadata.Should().Be("Test Metadata");
    }
}
