using AutoMapper;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Identity.UserProfiles.Queries; // Added for MappingProfile

namespace backend.Application.UnitTests.Members;

public class MemberDtoTests : IDisposable
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _configuration;

    public MemberDtoTests()
    {
        _configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfile).Assembly);
        });

        _mapper = _configuration.CreateMapper();
    }

    [Fact]
    public void ShouldHaveValidConfiguration()
    {
        _configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void ShouldMapMemberToMemberDto()
    {
        var member = new Member
        {
            FirstName = "John",
            LastName = "Doe",
            Nickname = "Johnny",
            DateOfBirth = new DateTime(1990, 1, 1),
            DateOfDeath = null,
            PlaceOfBirth = "New York",
            PlaceOfDeath = "Los Angeles",
            Gender = "Male",
            AvatarUrl = "http://example.com/avatar.jpg",
            Occupation = "Developer",
            Biography = "Some biography",
            FamilyId = Guid.NewGuid()
        };

        var memberDto = _mapper.Map<MemberDetailDto>(member);

        memberDto.Should().NotBeNull();
        memberDto.Id.Should().Be(member.Id);
        memberDto.FirstName.Should().Be(member.FirstName);
        memberDto.LastName.Should().Be(member.LastName);
        memberDto.FullName.Should().Be("Doe John");
        memberDto.Nickname.Should().Be(member.Nickname);
        memberDto.DateOfBirth.Should().Be(member.DateOfBirth);
        memberDto.DateOfDeath.Should().Be(member.DateOfDeath);
        memberDto.PlaceOfBirth.Should().Be(member.PlaceOfBirth);
        memberDto.PlaceOfDeath.Should().Be(member.PlaceOfDeath);
        memberDto.Gender.Should().Be(member.Gender);
        memberDto.AvatarUrl.Should().Be(member.AvatarUrl);
        memberDto.Occupation.Should().Be(member.Occupation);
        memberDto.Biography.Should().Be(member.Biography);
        memberDto.FamilyId.Should().Be(member.FamilyId);
    }

    public void Dispose()
    {
        // Clean up resources if needed
    }
}
