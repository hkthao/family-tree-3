using backend.Application.Common.Interfaces;
using backend.Application.Dashboard.Queries.GetPublicDashboard;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Dashboard.Queries.GetPublicDashboard;

public class GetPublicDashboardQueryHandlerTests : TestBase
{
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly GetPublicDashboardQueryHandler _handler;

    public GetPublicDashboardQueryHandlerTests()
    {
        _dateTimeMock = new Mock<IDateTime>();
        _handler = new GetPublicDashboardQueryHandler(_context, _dateTimeMock.Object);
        
        // Mock current date for age calculation
        _dateTimeMock.Setup(dt => dt.Now).Returns(new DateTime(2024, 1, 1));
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectPublicDashboardStats_WithMixedData()
    {
        // Arrange
        var publicFamilyId1 = Guid.NewGuid();
        var privateFamilyId1 = Guid.NewGuid();
        var publicFamilyId2 = Guid.NewGuid();

        var publicFamily1 = new Family { Id = publicFamilyId1, Name = "Public Family 1", Code = "PF1", Visibility = "Public" };
        var privateFamily1 = new Family { Id = privateFamilyId1, Name = "Private Family 1", Code = "PrF1", Visibility = "Private" };
        var publicFamily2 = new Family { Id = publicFamilyId2, Name = "Public Family 2", Code = "PF2", Visibility = "Public" };

        _context.Families.AddRange(publicFamily1, privateFamily1, publicFamily2);

        // Members for Public Family 1
        var memberPF1_Male_Living_Old = new Member("Doe", "John", "JMD1", publicFamilyId1, null, Gender.Male.ToString(), new DateTime(1950, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var memberPF1_Female_Living_Young = new Member("Doe", "Jane", "JFD1", publicFamilyId1, null, Gender.Female.ToString(), new DateTime(2000, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var memberPF1_Male_Deceased = new Member("Doe", "Jim", "JMD2", publicFamilyId1, null, Gender.Male.ToString(), new DateTime(1970, 1, 1), new DateTime(2020, 1, 1), null, null, null, null, null, null, null, null, null, true) { Id = Guid.NewGuid(), IsDeleted = true };
        var memberPF1_Female_Deleted = new Member("Doe", "Julia", "JFD2", publicFamilyId1, null, Gender.Female.ToString(), new DateTime(1980, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid(), IsDeleted = true };
        
        var memberPF2_Male_Living = new Member("Smith", "Peter", "SP1", publicFamilyId2, null, Gender.Male.ToString(), new DateTime(1990, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };

                _context.Members.AddRange(

                    memberPF1_Male_Living_Old,

                    memberPF1_Female_Living_Young,

                    memberPF1_Male_Deceased,

                    memberPF1_Female_Deleted,

                    memberPF2_Male_Living,

                    new Member("Member", "Private", "PM1", privateFamilyId1, null, Gender.Female.ToString(), new DateTime(1960, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() }

                );

        

                _context.Relationships.Add(new Relationship(publicFamilyId1, memberPF1_Male_Living_Old.Id, memberPF1_Female_Living_Young.Id, RelationshipType.Father));

                

                // Events for Public Family 1
        var eventPF1_Public = new Event("Public Event", "PE1", EventType.Other, publicFamilyId1) { Id = Guid.NewGuid() };
        var eventPF1_Deleted = new Event("Deleted Event", "DE1", EventType.Other, publicFamilyId1) { Id = Guid.NewGuid(), IsDeleted = true };
        _context.Events.AddRange(eventPF1_Public, eventPF1_Deleted);

        await _context.SaveChangesAsync();

        var query = new GetPublicDashboardQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value!.TotalPublicFamilies.Should().Be(2); // publicFamily1, publicFamily2
        result.Value.TotalPublicMembers.Should().Be(3); // memberPF1_Male_Living_Old, memberPF1_Female_Living_Young, memberPF2_Male_Living (memberPF1_Female_Deleted is soft-deleted)
        result.Value.TotalPublicRelationships.Should().Be(1); // relPF1_ParentChild1
        result.Value.TotalPublicEvents.Should().Be(1); // eventPF1_Public (eventPF1_Deleted is soft-deleted)

        // Gender Ratios (from 3 members: 2 male, 1 female)
        result.Value.PublicMaleRatio.Should().BeApproximately(2.0 / 3.0, 0.001);
        result.Value.PublicFemaleRatio.Should().BeApproximately(1.0 / 3.0, 0.001);

        // Living and Deceased
        result.Value.PublicLivingMembersCount.Should().Be(3); // memberPF1_Male_Living_Old, memberPF1_Female_Living_Young, memberPF2_Male_Living
        result.Value.PublicDeceasedMembersCount.Should().Be(0); // memberPF1_Male_Deceased is deceased but also filtered by IsDeleted=false

        // Average Age (as of 2024-01-01)
        // memberPF1_Male_Living_Old: 2024 - 1950 = 74
        // memberPF1_Female_Living_Young: 2024 - 2000 = 24
        // memberPF2_Male_Living: 2024 - 1990 = 34
        // Average: (74 + 24 + 34) / 3 = 132 / 3 = 44
        result.Value.PublicAverageAge.Should().Be(44);

        // Generations (Public Family 1 has 2 generations based on relPF1_ParentChild1)
        // Public Family 2 has 1 generation
        result.Value.TotalPublicGenerations.Should().Be(2); // Max generations across public families
        result.Value.PublicMembersPerGeneration.Should().HaveCount(2);
        result.Value.PublicMembersPerGeneration[1].Should().Be(2); // memberPF1_Male_Living_Old, memberPF2_Male_Living
        result.Value.PublicMembersPerGeneration[2].Should().Be(1); // memberPF1_Female_Living_Young
    }

    [Fact]
    public async Task Handle_ShouldReturnZeroStats_WhenNoPublicData()
    {
        // Arrange
        var privateFamily1 = new Family { Id = Guid.NewGuid(), Name = "Private Family 1", Code = "PrF1", Visibility = "Private" };
        _context.Families.Add(privateFamily1);
        _context.Members.Add(new Member("Member", "Private", "PM1", privateFamily1.Id, null, Gender.Female.ToString(), new DateTime(1960, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() });
        _context.Events.Add(new Event("Private Event", "PrE1", EventType.Other, privateFamily1.Id) { Id = Guid.NewGuid() });
        await _context.SaveChangesAsync();

        var query = new GetPublicDashboardQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalPublicFamilies.Should().Be(0);
        result.Value.TotalPublicMembers.Should().Be(0);
        result.Value.TotalPublicRelationships.Should().Be(0);
        result.Value.TotalPublicEvents.Should().Be(0);
        result.Value.PublicMaleRatio.Should().Be(0);
        result.Value.PublicFemaleRatio.Should().Be(0);
        result.Value.PublicLivingMembersCount.Should().Be(0);
        result.Value.PublicDeceasedMembersCount.Should().Be(0);
        result.Value.PublicAverageAge.Should().Be(0);
        result.Value.PublicMembersPerGeneration.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCalculateGenerationsCorrectly_ForASinglePublicFamily()
    {
        // Arrange
        var publicFamilyId = Guid.NewGuid();
        var publicFamily = new Family { Id = publicFamilyId, Name = "Public Family Gen Test", Code = "PFGT", Visibility = "Public" };
        _context.Families.Add(publicFamily);

        // Generation 1
        var gen1_member1 = new Member("M1", "Gen1", "G1M1", publicFamilyId, null, Gender.Male.ToString(), new DateTime(1950, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var gen1_member2 = new Member("M2", "Gen1", "G1M2", publicFamilyId, null, Gender.Male.ToString(), new DateTime(1955, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        _context.Members.AddRange(gen1_member1, gen1_member2);


        // Generation 2
        var gen2_member1 = new Member("M1", "Gen2", "G2M1", publicFamilyId, null, Gender.Female.ToString(), new DateTime(1980, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var gen2_member2 = new Member("M2", "Gen2", "G2M2", publicFamilyId, null, Gender.Male.ToString(), new DateTime(1982, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        _context.Members.AddRange(gen2_member1, gen2_member2);


        // Generation 3
        var gen3_member1 = new Member("M1", "Gen3", "G3M1", publicFamilyId, null, Gender.Male.ToString(), new DateTime(2010, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        _context.Members.Add(gen3_member1);


        // Relationships
        // G1M1 -> G2M1 (Father)
        _context.Relationships.Add(new Relationship(publicFamilyId, gen1_member1.Id, gen2_member1.Id, RelationshipType.Father));
        // G1M2 -> G2M2 (Father)
        _context.Relationships.Add(new Relationship(publicFamilyId, gen1_member2.Id, gen2_member2.Id, RelationshipType.Father));
        // G2M1 -> G3M1 (Mother)
        _context.Relationships.Add(new Relationship(publicFamilyId, gen2_member1.Id, gen3_member1.Id, RelationshipType.Mother));

        await _context.SaveChangesAsync();

        var query = new GetPublicDashboardQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalPublicGenerations.Should().Be(3);
        result.Value.PublicMembersPerGeneration.Should().HaveCount(3);
        result.Value.PublicMembersPerGeneration[1].Should().Be(2); // gen1_member1, gen1_member2
        result.Value.PublicMembersPerGeneration[2].Should().Be(2); // gen2_member1, gen2_member2
        result.Value.PublicMembersPerGeneration[3].Should().Be(1); // gen3_member1
    }

    [Fact]
    public async Task Handle_ShouldNotCountDeletedEntities()
    {
        // Arrange
        var publicFamilyId = Guid.NewGuid();
        var publicFamily = new Family { Id = publicFamilyId, Name = "Public Family Deleted", Code = "PFD", Visibility = "Public" };
        _context.Families.Add(publicFamily);

        var deletedMember = new Member("Mem", "Del", "DM1", publicFamilyId, null, Gender.Male.ToString(), new DateTime(1990, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid(), IsDeleted = true };
        var activeMember = new Member("Mem", "Act", "AM1", publicFamilyId, null, Gender.Male.ToString(), new DateTime(1990, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        _context.Members.AddRange(deletedMember, activeMember);


        var deletedRelationship = new Relationship(publicFamilyId, activeMember.Id, deletedMember.Id, RelationshipType.Husband) { Id = Guid.NewGuid(), IsDeleted = true }; // Using Husband as a valid type
        var activeRelationship = new Relationship(publicFamilyId, activeMember.Id, activeMember.Id, RelationshipType.Father) { Id = Guid.NewGuid() }; // Using Father as a valid type
        _context.Relationships.AddRange(deletedRelationship, activeRelationship);
        
        var deletedEvent = new Event("Del Event", "DE1", EventType.Other, publicFamilyId) { Id = Guid.NewGuid(), IsDeleted = true };
        var activeEvent = new Event("Act Event", "AE1", EventType.Other, publicFamilyId) { Id = Guid.NewGuid() };
        _context.Events.AddRange(deletedEvent, activeEvent);

        await _context.SaveChangesAsync();

        var query = new GetPublicDashboardQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalPublicMembers.Should().Be(1); // Only activeMember
        result.Value.TotalPublicRelationships.Should().Be(1); // Only activeRelationship
        result.Value.TotalPublicEvents.Should().Be(1); // Only activeEvent
    }
}
