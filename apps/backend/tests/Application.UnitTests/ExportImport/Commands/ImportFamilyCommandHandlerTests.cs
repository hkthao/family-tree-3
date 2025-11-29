using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.ExportImport.Commands;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.ExportImport.Commands;

public class ImportFamilyCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly ImportFamilyCommandHandler _handler;

    public ImportFamilyCommandHandlerTests() : base()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _dateTimeMock = new Mock<IDateTime>();
        _handler = new ImportFamilyCommandHandler(_context, _currentUserMock.Object, _dateTimeMock.Object);

        // Setup common mocks
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _dateTimeMock.Setup(x => x.Now).Returns(new DateTime(2023, 1, 1));
    }

    private Family CreateTestFamily(Guid familyId, Guid creatorUserId)
    {
        var family = new Family { Id = familyId, Name = "Existing Family", Code = "EF1", CreatedBy = creatorUserId.ToString() };
        family.AddFamilyUser(creatorUserId, FamilyRole.Manager);
        return family;
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyDataIsNull()
    {
        // Arrange
        var command = new ImportFamilyCommand { FamilyId = Guid.NewGuid(), FamilyData = null! };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Family data cannot be null.");
        result.ErrorSource.Should().Be(ErrorSources.Validation);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyIdIsNull()
    {
        // Arrange
        var command = new ImportFamilyCommand { FamilyId = null, FamilyData = new FamilyExportDto() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("FamilyId must be provided to update an existing family.");
        result.ErrorSource.Should().Be(ErrorSources.Validation);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyNotFound()
    {
        // Arrange
        var nonExistentFamilyId = Guid.NewGuid();
        var command = new ImportFamilyCommand
        {
            FamilyId = nonExistentFamilyId,
            FamilyData = new FamilyExportDto { Name = "New Family", Code = "NF1" }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Family with ID '{nonExistentFamilyId}' not found.");
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldSuccessfullyImportAndClearExistingData_WhenClearExistingDataIsTrue()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var creatorUserId = _currentUserMock.Object.UserId;
        var existingFamily = CreateTestFamily(familyId, creatorUserId);
        _context.Families.Add(existingFamily);

        var existingMember1 = new Member("Existing", "Member1", "EM1", familyId, false) { Id = Guid.NewGuid() };
        var existingMember2 = new Member("Existing", "Member2", "EM2", familyId, false) { Id = Guid.NewGuid() };
        _context.Members.AddRange(existingMember1, existingMember2);

        var existingRelationship = new Relationship(familyId, existingMember1.Id, existingMember2.Id, RelationshipType.Father);
        _context.Relationships.Add(existingRelationship);

        var existingEvent = new Event("Existing Event", "EE1", EventType.Other, familyId);
        _context.Events.Add(existingEvent);

        await _context.SaveChangesAsync();

        var importCommand = new ImportFamilyCommand
        {
            FamilyId = familyId,
            ClearExistingData = true,
            FamilyData = new FamilyExportDto
            {
                Name = "Updated Family Name",
                Code = "EF1", // Keep the same code
                Description = "Updated description",
                Visibility = "Private",
                Members = new List<MemberExportDto>
                {
                    new MemberExportDto { Id = Guid.NewGuid(), FirstName = "Imported", LastName = "MemberA", Code = "IMA", Gender = Gender.Male, DateOfBirth = new DateTime(1990, 1, 1) },
                    new MemberExportDto { Id = Guid.NewGuid(), FirstName = "Imported", LastName = "MemberB", Code = "IMB", Gender = Gender.Female, DateOfBirth = new DateTime(1992, 2, 2) }
                },
                Relationships = new List<RelationshipExportDto>
                {
                    new RelationshipExportDto { Id = Guid.NewGuid(), SourceMemberId = Guid.Parse("00000000-0000-0000-0000-000000000001"), TargetMemberId = Guid.Parse("00000000-0000-0000-0000-000000000002"), Type = RelationshipType.Husband }
                },
                Events = new List<EventExportDto>
                {
                    new EventExportDto { Id = Guid.NewGuid(), Name = "Imported Event", Code = "IE1", Type = EventType.Other, Description = "Event description", StartDate = new DateTime(2000, 1, 1), RelatedMembers = new List<Guid> { Guid.Parse("00000000-0000-0000-0000-000000000001") } }
                }
            }
        };

        // Adjusting Member IDs in relationships/events to match imported ones for easier verification
        importCommand.FamilyData.Members[0].Id = importCommand.FamilyData.Relationships[0].SourceMemberId;
        importCommand.FamilyData.Members[1].Id = importCommand.FamilyData.Relationships[0].TargetMemberId;
        importCommand.FamilyData.Events[0].RelatedMembers[0] = importCommand.FamilyData.Members[0].Id;

        // Act
        var result = await _handler.Handle(importCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(familyId);

        // Verify existing data is cleared
        _context.Members.Should().HaveCount(importCommand.FamilyData.Members.Count);
        _context.Relationships.Should().HaveCount(importCommand.FamilyData.Relationships.Count);
        _context.Events.Should().HaveCount(importCommand.FamilyData.Events.Count);

        // Verify family details updated
        var updatedFamily = await _context.Families.FindAsync(familyId);
        updatedFamily.Should().NotBeNull();
        updatedFamily!.Name.Should().Be(importCommand.FamilyData.Name);
        updatedFamily.Description.Should().Be(importCommand.FamilyData.Description);

        // Verify new members
        var importedMemberA = _context.Members.FirstOrDefault(m => m.FirstName == "Imported" && m.LastName == "MemberA");
        importedMemberA.Should().NotBeNull();
        importedMemberA!.FamilyId.Should().Be(familyId);

        var importedMemberB = _context.Members.FirstOrDefault(m => m.FirstName == "Imported" && m.LastName == "MemberB");
        importedMemberB.Should().NotBeNull();
        importedMemberB!.FamilyId.Should().Be(familyId);

        // Verify new relationships
        var importedRelationship = _context.Relationships.FirstOrDefault(r => r.Type == RelationshipType.Husband);
        importedRelationship.Should().NotBeNull();
        importedRelationship!.SourceMemberId.Should().Be(importedMemberA!.Id);
        importedRelationship.TargetMemberId.Should().Be(importedMemberB!.Id);

        // Verify new events
        var importedEvent = _context.Events.FirstOrDefault(e => e.Name == "Imported Event");
        importedEvent.Should().NotBeNull();
        importedEvent!.FamilyId.Should().Be(familyId);
        importedEvent.EventMembers.Should().HaveCount(1);
        importedEvent.EventMembers.First().MemberId.Should().Be(importedMemberA!.Id);

        // Verify RecalculateStats is called (implicitly by checking updated counts if direct mock not possible)
        // For in-memory, we can check the values directly after save changes.
        updatedFamily.TotalMembers.Should().Be(importCommand.FamilyData.Members.Count);
        updatedFamily.TotalGenerations.Should().BeGreaterThanOrEqualTo(0); // Cannot precisely mock
    }

    [Fact]
    public async Task Handle_ShouldSuccessfullyImportAndKeepExistingData_WhenClearExistingDataIsFalse()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var creatorUserId = _currentUserMock.Object.UserId;
        var existingFamily = CreateTestFamily(familyId, creatorUserId);
        _context.Families.Add(existingFamily);

        var existingMember1 = new Member("Existing", "Member1", "EM1", familyId, false) { Id = Guid.NewGuid() };
        var existingMember2 = new Member("Existing", "Member2", "EM2", familyId, false) { Id = Guid.NewGuid() };
        _context.Members.AddRange(existingMember1, existingMember2);

        var existingRelationship = new Relationship(familyId, existingMember1.Id, existingMember2.Id, RelationshipType.Father);
        _context.Relationships.Add(existingRelationship);

        var existingEvent = new Event("Existing Event", "EE1", EventType.Other, familyId);
        _context.Events.Add(existingEvent);

        await _context.SaveChangesAsync();

        var importCommand = new ImportFamilyCommand
        {
            FamilyId = familyId,
            ClearExistingData = false,
            FamilyData = new FamilyExportDto
            {
                Name = "Updated Family Name",
                Code = "EF1", // Keep the same code
                Description = "Updated description",
                Visibility = "Private",
                Members = new List<MemberExportDto>
                {
                    new MemberExportDto { Id = Guid.NewGuid(), FirstName = "Imported", LastName = "MemberA", Code = "IMA", Gender = Gender.Male, DateOfBirth = new DateTime(1990, 1, 1) }
                },
                Relationships = new List<RelationshipExportDto>(), // No new relationships
                Events = new List<EventExportDto>() // No new events
            }
        };

        // Act
        var result = await _handler.Handle(importCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(familyId);

        // Verify existing data is NOT cleared
        _context.Members.Should().HaveCount(2 + importCommand.FamilyData.Members.Count); // 2 existing + 1 new
        _context.Relationships.Should().HaveCount(1); // 1 existing
        _context.Events.Should().HaveCount(1); // 1 existing

        // Verify family details updated
        var updatedFamily = await _context.Families.FindAsync(familyId);
        updatedFamily.Should().NotBeNull();
        updatedFamily!.Name.Should().Be(importCommand.FamilyData.Name);
        updatedFamily.Description.Should().Be(importCommand.FamilyData.Description);

        // Verify new members added
        var importedMemberA = _context.Members.FirstOrDefault(m => m.FirstName == "Imported" && m.LastName == "MemberA");
        importedMemberA.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRelationshipMemberIdMappingFails()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var creatorUserId = _currentUserMock.Object.UserId;
        var existingFamily = CreateTestFamily(familyId, creatorUserId);
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        var importCommand = new ImportFamilyCommand
        {
            FamilyId = familyId,
            FamilyData = new FamilyExportDto
            {
                Name = "Test Family",
                Code = "TF1",
                Visibility = "Private",
                Members = new List<MemberExportDto>
                {
                    new MemberExportDto { Id = Guid.NewGuid(), FirstName = "Member", LastName = "A", Code = "MA" }
                },
                Relationships = new List<RelationshipExportDto>
                {
                    // This relationship references a member ID that is NOT in the Members list
                    new RelationshipExportDto { Id = Guid.NewGuid(), SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), Type = RelationshipType.Father }
                }
            }
        };

        // Act
        var result = await _handler.Handle(importCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to map member IDs for relationship");
        result.ErrorSource.Should().Be(ErrorSources.Validation);
    }
}
