using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Families.ExportImport;
using backend.Application.Families.Queries.ExportImport;
using backend.Application.Families.Specifications;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Models; // For Result
using Ardalis.Specification.EntityFrameworkCore; // For WithSpecification
using backend.Domain.Enums; // For Enums
using Microsoft.EntityFrameworkCore; // For AsNoTracking, FirstOrDefaultAsync
using backend.Application.UnitTests.Common; // For TestBase
using backend.Application.Common.Constants; // For ErrorMessages

namespace backend.Application.UnitTests.Families.Queries.ExportImport;

public class GetFamilyExportQueryHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetFamilyExportQueryHandler _handler;

    public GetFamilyExportQueryHandlerTests()
    {
        _mockMapper = new Mock<IMapper>();
        _handler = new GetFamilyExportQueryHandler(_context, _mockMapper.Object, _mockAuthorizationService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFamilyExportDto_WhenFamilyExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var creatorUserId = Guid.NewGuid();
        var family = Family.Create("Test Family", "TF1", "Description", "Address", "AvatarUrl", "Private", creatorUserId);
        family.Id = familyId;

        var member1 = new Member("Doe", "John", "JOHNDOE", familyId, "Johnny", "Male", DateTime.Now.AddYears(-30), null, "Place1", null, null, null, null, "Occupation1", "Avatar1", "Bio1", 1);
        member1.SetId(Guid.NewGuid());
        var member2 = new Member("Doe", "Jane", "JANEDOE", familyId, "Janie", "Female", DateTime.Now.AddYears(-28), null, "Place2", null, null, null, null, "Occupation2", "Avatar2", "Bio2", 2);
        member2.SetId(Guid.NewGuid());

        var relationship = new Relationship(familyId, member1.Id, member2.Id, RelationshipType.Husband, 1);
        relationship.Id = Guid.NewGuid();

        var event1 = new Event("Wedding", "WEDDING", EventType.Marriage, familyId, DateTime.Now.AddYears(-5));
        event1.Id = Guid.NewGuid();
        event1.AddEventMember(member1.Id);
        event1.AddEventMember(member2.Id);

        // Manually add members, relationships, and events to family's private collections for testing purposes
        // In a real scenario, these would be added via Family's public methods or loaded from DB
        var membersField = typeof(Family).GetField("_members", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var relationshipsField = typeof(Family).GetField("_relationships", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var eventsField = typeof(Family).GetField("_events", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (membersField != null)
        {
            var membersCollection = membersField.GetValue(family) as HashSet<Member>;
            membersCollection?.Add(member1);
            membersCollection?.Add(member2);
        }
        if (relationshipsField != null)
        {
            var relationshipsCollection = relationshipsField.GetValue(family) as HashSet<Relationship>;
            relationshipsCollection?.Add(relationship);
        }
        if (eventsField != null)
        {
            var eventsCollection = eventsField.GetValue(family) as HashSet<Event>;
            eventsCollection?.Add(event1);
        }


        FamilyExportDto familyExportDto = new FamilyExportDto
        {
            Id = family.Id,
            Name = family.Name,
            Description = family.Description,
            Address = family.Address,
            Visibility = family.Visibility,
            Members = new List<MemberExportDto>
            {
                new MemberExportDto { Id = member1.Id, FirstName = member1.FirstName, LastName = member1.LastName, Code = member1.Code, Gender = (Gender)Enum.Parse(typeof(Gender), member1.Gender!), IsRoot = member1.IsRoot, Order = member1.Order },
                new MemberExportDto { Id = member2.Id, FirstName = member2.FirstName, LastName = member2.LastName, Code = member2.Code, Gender = (Gender)Enum.Parse(typeof(Gender), member2.Gender!), IsRoot = member2.IsRoot, Order = member2.Order }
            },
            Relationships = new List<RelationshipExportDto>
            {
                new RelationshipExportDto { Id = relationship.Id, SourceMemberId = relationship.SourceMemberId, TargetMemberId = relationship.TargetMemberId, Type = relationship.Type, Order = relationship.Order }
            },
            Events = new List<EventExportDto>
            {
                new EventExportDto { Id = event1.Id, Name = event1.Name, Type = event1.Type, RelatedMembers = event1.EventMembers.Select(em => em.MemberId).ToList() }
            }
        };

        // Use _context from TestBase
        _context.Families.Add(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Mock mapper to return the DTO
        _mockMapper.Setup(m => m.Map<FamilyExportDto>(It.IsAny<Family>()))
            .Returns(familyExportDto);

        _mockAuthorizationService.Setup(a => a.CanAccessFamily(familyId)).Returns(true);

        var query = new GetFamilyExportQuery(familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(familyExportDto);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenFamilyDoesNotExist()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        _mockAuthorizationService.Setup(a => a.CanAccessFamily(familyId)).Returns(true);

        var query = new GetFamilyExportQuery(familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.FamilyNotFound, familyId));
    }

    [Fact]
    public async Task Handle_ShouldReturnAccessDenied_WhenUserCannotAccessFamily()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        _mockAuthorizationService.Setup(a => a.CanAccessFamily(familyId)).Returns(false);

        var query = new GetFamilyExportQuery(familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied");
    }
}