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
            Status = "Active",
            Phone = "123-456-7890",
            Email = "john.doe@example.com",
            Generation = 1,
            DisplayOrder = 1,
            FamilyId = ObjectId.GenerateNewId(), // Assuming ObjectId for FamilyId
            Description = "Some description"
        };

        var memberDto = _mapper.Map<MemberDto>(member);

        memberDto.Should().NotBeNull();
        memberDto.Id.Should().Be(member.Id);
        memberDto.FullName.Should().Be(member.FullName);
        memberDto.DateOfBirth.Should().Be(member.DateOfBirth);
        memberDto.DateOfDeath.Should().Be(member.DateOfDeath);
        memberDto.Status.Should().Be(member.Status);
        memberDto.Phone.Should().Be(member.Phone);
        memberDto.Email.Should().Be(member.Email);
        memberDto.Generation.Should().Be(member.Generation);
        memberDto.DisplayOrder.Should().Be(member.DisplayOrder);
        memberDto.FamilyId.Should().Be(member.FamilyId.ToString());
        memberDto.Description.Should().Be(member.Description);
    }

    public void Dispose()
    {
        // Clean up resources if needed
    }
}