using AutoMapper;
using backend.Application.Members;
using backend.Domain.Entities;
using Xunit;
using FluentAssertions;
using System;
using backend.Application.Common.Mappings; 
using MongoDB.Bson;

namespace backend.Application.UnitTests.Members;

public class MemberDtoTests : IDisposable
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _configuration;

    public MemberDtoTests()
    {
        _configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(backend.Application.Common.Mappings.IMapFrom<>).Assembly); // Scan the assembly for profiles
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
            // Update with new Member properties
            FullName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            DateOfDeath = null,
            PlaceOfBirth = "New York",
            Gender = "Male",
            AvatarUrl = "http://example.com/avatar.jpg",
            Phone = "123-456-7890",
            Email = "john.doe@example.com",
            Generation = 1,
            Biography = "Some biography",
            Metadata = new { Key = "Value" }
        };

        var memberDto = _mapper.Map<MemberDto>(member);

        memberDto.Should().NotBeNull();
        memberDto.Id.Should().Be(member.Id);
        memberDto.FullName.Should().Be(member.FullName);
        memberDto.DateOfBirth.Should().Be(member.DateOfBirth);
        memberDto.DateOfDeath.Should().Be(member.DateOfDeath);
        memberDto.Gender.Should().Be(member.Gender);
        memberDto.Phone.Should().Be(member.Phone);
        memberDto.PlaceOfBirth.Should().Be(member.PlaceOfBirth);
        memberDto.Generation.Should().Be(member.Generation);
        memberDto.AvatarUrl.Should().Be(member.AvatarUrl);
        memberDto.Biography.Should().Be(member.Biography);
        memberDto.Metadata.Should().BeEquivalentTo(member.Metadata);
    }

    public void Dispose()
    {
        // Clean up resources if needed
    }
}