using AutoMapper;
using backend.Application.Members;
using backend.Domain.Entities;
using NUnit.Framework;
using Shouldly;
using System;
using System.Reflection; // Added for Assembly

namespace backend.Application.UnitTests.Members;

public class MemberDtoTests
{
    private IMapper _mapper;

    [SetUp]
    public void Setup()
    {
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            // Scan for IMapFrom profiles in the Application assembly
            cfg.CreateMap<Member, MemberDto>();
        });

        _mapper = configurationProvider.CreateMapper();
    }

    [Test]
    public void Properties_ShouldBeSettableAndGettable()
    {
        var dto = new MemberDto
        {
            Id = "60c72b2f9b1e8c001c8e4d0a",
            FullName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            DateOfDeath = null,
            Status = "Active",
            Phone = "123-456-7890",
            Email = "john.doe@example.com",
            Generation = 1,
            DisplayOrder = 10,
            FamilyId = "60c72b2f9b1e8c001c8e4d0b",
            Description = "Test description"
        };

        dto.Id.ShouldBe("60c72b2f9b1e8c001c8e4d0a");
        dto.FullName.ShouldBe("John Doe");
        dto.DateOfBirth.ShouldBe(new DateTime(1990, 1, 1));
        dto.DateOfDeath.ShouldBeNull();
        dto.Status.ShouldBe("Active");
        dto.Phone.ShouldBe("123-456-7890");
        dto.Email.ShouldBe("john.doe@example.com");
        dto.Generation.ShouldBe(1);
        dto.DisplayOrder.ShouldBe(10);
        dto.FamilyId.ShouldBe("60c72b2f9b1e8c001c8e4d0b");
        dto.Description.ShouldBe("Test description");
    }

    [Test]
    public void Mapping_ShouldMapMemberToMemberDto()
    {
        var member = new Member
        {
            FullName = "Jane Doe",
            DateOfBirth = new DateTime(1992, 5, 10),
            DateOfDeath = null,
            Status = "Inactive",
            Phone = "098-765-4321",
            Email = "jane.doe@example.com",
            Generation = 2,
            DisplayOrder = 20,
            FamilyId = MongoDB.Bson.ObjectId.GenerateNewId(),
            Description = "Another test description"
        };

        var memberDto = _mapper.Map<MemberDto>(member);

        memberDto.FullName.ShouldBe(member.FullName);
        memberDto.DateOfBirth.ShouldBe(member.DateOfBirth);
        memberDto.DateOfDeath.ShouldBe(member.DateOfDeath);
        memberDto.Status.ShouldBe(member.Status);
        memberDto.Phone.ShouldBe(member.Phone);
        memberDto.Email.ShouldBe(member.Email);
        memberDto.Generation.ShouldBe(member.Generation);
        memberDto.DisplayOrder.ShouldBe(member.DisplayOrder);
        memberDto.FamilyId.ShouldBe(member.FamilyId.ToString());
        memberDto.Description.ShouldBe(member.Description);
    }
}