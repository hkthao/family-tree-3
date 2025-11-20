using AutoMapper;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Queries.GetPublicMembersByFamilyId;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Queries.GetPublicMembersByFamilyId;

public class GetPublicMembersByFamilyIdQueryHandlerTests : TestBase
{
    private readonly GetPublicMembersByFamilyIdQueryHandler _handler;
    private readonly Mock<IPrivacyService> _mockPrivacyService;

    public GetPublicMembersByFamilyIdQueryHandlerTests()
    {
        _mockPrivacyService = new Mock<IPrivacyService>();
        _handler = new GetPublicMembersByFamilyIdQueryHandler(_context, _mapper, _mockPrivacyService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMembers_WhenFamilyExistsAndIsPublic()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var publicFamily = new Family { Id = familyId, Name = "Public Family", Code = "PUB1", Visibility = FamilyVisibility.Public.ToString() };
        _context.Families.Add(publicFamily);

        var member1 = new Member("Doe", "John", "M1", familyId) { Id = Guid.NewGuid() };
        var member2 = new Member("Doe", "Jane", "M2", familyId) { Id = Guid.NewGuid() };
        _context.Members.AddRange(member1, member2);
        await _context.SaveChangesAsync();

        // Mock privacy service to return members as is for this test
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<List<MemberListDto>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<MemberListDto> members, Guid fId, CancellationToken ct) => members);

        var query = new GetPublicMembersByFamilyIdQuery(familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        result.Value.Should().Contain(m => m.Id == member1.Id);
        result.Value.Should().Contain(m => m.Id == member2.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyDoesNotExist()
    {
        // Arrange
        var nonExistentFamilyId = Guid.NewGuid();
        var query = new GetPublicMembersByFamilyIdQuery(nonExistentFamilyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.FamilyNotFound, nonExistentFamilyId));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyExistsButIsPrivate()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var privateFamily = new Family { Id = familyId, Name = "Private Family", Code = "PRIV1", Visibility = FamilyVisibility.Private.ToString() };
        _context.Families.Add(privateFamily);
        await _context.SaveChangesAsync();

        var query = new GetPublicMembersByFamilyIdQuery(familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldApplyPrivacyFilters()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var publicFamily = new Family { Id = familyId, Name = "Public Family", Code = "PUB1", Visibility = FamilyVisibility.Public.ToString() };
        _context.Families.Add(publicFamily);

        var member1 = new Member("Doe", "John", "M1", familyId) { Id = Guid.NewGuid() };
        _context.Members.Add(member1);
        await _context.SaveChangesAsync();

        var filteredMemberDto = new MemberListDto { Id = member1.Id, FirstName = "John", LastName = "" }; // Example of filtered data
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<List<MemberListDto>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<MemberListDto> members, Guid fId, CancellationToken ct) =>
            {
                members.ForEach(m =>
                {
                    m.LastName = "";
                });
                return members;
            });

        var query = new GetPublicMembersByFamilyIdQuery(familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull().And.NotBeEmpty(); // Ensure not null and not empty
        var actualMember = result.Value!.First(); // Assign to a variable after null/empty check
        actualMember.Id.Should().Be(member1.Id);
        actualMember.FirstName.Should().Be("John");
        actualMember.LastName.Should().Be(""); // Assert that privacy filter was applied
        _mockPrivacyService.Verify(x => x.ApplyPrivacyFilter(It.IsAny<List<MemberListDto>>(), familyId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPopulateRelationshipIdsCorrectly()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var publicFamily = new Family { Id = familyId, Name = "Public Family", Code = "PUB_REL", Visibility = FamilyVisibility.Public.ToString() };
        _context.Families.Add(publicFamily);

        // Use the constructor that allows setting gender
        var father = new Member("Test", "Father", "F1", familyId, null, Gender.Male.ToString(), null, null, null, null, null, null, null, null, null, null, null) { Id = Guid.NewGuid() };
        var mother = new Member("Test", "Mother", "M1", familyId, null, Gender.Female.ToString(), null, null, null, null, null, null, null, null, null, null, null) { Id = Guid.NewGuid() };
        var child = new Member("Test", "Child", "C1", familyId, null, Gender.Male.ToString(), null, null, null, null, null, null, null, null, null, null, null) { Id = Guid.NewGuid() };
        var spouse = new Member("Test", "Spouse", "S1", familyId, null, Gender.Female.ToString(), null, null, null, null, null, null, null, null, null, null, null) { Id = Guid.NewGuid() };

        _context.Members.AddRange(father, mother, child, spouse);

        // Relationships - use the public constructor
        _context.Relationships.Add(new Relationship(familyId, father.Id, child.Id, RelationshipType.Father));
        _context.Relationships.Add(new Relationship(familyId, mother.Id, child.Id, RelationshipType.Mother));
        _context.Relationships.Add(new Relationship(familyId, father.Id, spouse.Id, RelationshipType.Husband)); // Father is husband of spouse
        _context.Relationships.Add(new Relationship(familyId, spouse.Id, father.Id, RelationshipType.Wife)); // Spouse is wife of father

        await _context.SaveChangesAsync();

        // Debugging: Inspect relationships directly from the context
        var savedFather = _context.Members.Include(m => m.SourceRelationships).Include(m => m.TargetRelationships).FirstOrDefault(m => m.Id == father.Id);
        var savedMother = _context.Members.Include(m => m.SourceRelationships).Include(m => m.TargetRelationships).FirstOrDefault(m => m.Id == mother.Id);
        var savedChild = _context.Members.Include(m => m.SourceRelationships).Include(m => m.TargetRelationships).FirstOrDefault(m => m.Id == child.Id);
        var savedSpouse = _context.Members.Include(m => m.SourceRelationships).Include(m => m.TargetRelationships).FirstOrDefault(m => m.Id == spouse.Id);

        savedFather.Should().NotBeNull();
        savedFather!.SourceRelationships.Should().ContainSingle(r => r.Type == RelationshipType.Husband && r.TargetMemberId == spouse.Id);
        savedFather.SourceRelationships.Should().ContainSingle(r => r.Type == RelationshipType.Father && r.SourceMemberId == father.Id && r.TargetMemberId == child.Id);

        savedMother.Should().NotBeNull();
        savedMother!.SourceRelationships.Should().ContainSingle(r => r.Type == RelationshipType.Mother && r.SourceMemberId == mother.Id && r.TargetMemberId == child.Id);
        savedMother.TargetRelationships.Should().BeEmpty(); // Mother is not target of any relationship in this setup

        savedChild.Should().NotBeNull();
        savedChild!.SourceRelationships.Should().BeEmpty(); // Child is not source of any relationship in this setup
        savedChild.TargetRelationships.Should().HaveCount(2); // Father and Mother relationships

        savedSpouse.Should().NotBeNull();
        savedSpouse!.SourceRelationships.Should().ContainSingle(r => r.Type == RelationshipType.Wife && r.TargetMemberId == father.Id);
        savedSpouse.TargetRelationships.Should().ContainSingle(r => r.Type == RelationshipType.Husband && r.SourceMemberId == father.Id && r.TargetMemberId == spouse.Id);

        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<List<MemberListDto>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<MemberListDto> members, Guid fId, CancellationToken ct) => members);

        var query = new GetPublicMembersByFamilyIdQuery(familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var childDto = result.Value!.FirstOrDefault(m => m.Id == child.Id);
        childDto.Should().NotBeNull();
        childDto!.FatherId.Should().Be(father.Id);
        childDto.MotherId.Should().Be(mother.Id);
        childDto.HusbandId.Should().BeNull(); // Child is not a husband
        childDto.WifeId.Should().BeNull();    // Child is not a wife

        var fatherDto = result.Value!.FirstOrDefault(m => m.Id == father.Id);
        fatherDto.Should().NotBeNull();
        fatherDto!.FatherId.Should().BeNull(); // Father has no father in this setup
        fatherDto.MotherId.Should().BeNull(); // Father has no mother in this setup
        fatherDto.HusbandId.Should().BeNull(); // Father is husband, but this is for the member's husband
        fatherDto.WifeId.Should().Be(spouse.Id); // Father's wife is spouse

        var spouseDto = result.Value!.FirstOrDefault(m => m.Id == spouse.Id);
        spouseDto.Should().NotBeNull();
        spouseDto!.FatherId.Should().BeNull();
        spouseDto.MotherId.Should().BeNull();
        spouseDto.HusbandId.Should().Be(father.Id); // Spouse's husband is father
        spouseDto.WifeId.Should().BeNull();
    }
}

