using AutoMapper;
using backend.Application.Common.Constants; // Add this using statement
using backend.Application.Common.Interfaces; // Add this using statement
using backend.Application.ExportImport.Commands;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;


namespace backend.Application.UnitTests.Families.Commands.ExportImport;

public class ImportFamilyCommandHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly ImportFamilyCommandHandler _handler;

    public ImportFamilyCommandHandlerTests()
    {
        _mockMapper = new Mock<IMapper>(); // Initialize mock mapper
        _handler = new ImportFamilyCommandHandler(_context, _mockUser.Object, _mockDateTime.Object);
    }

    private Family CreateTestFamily(Guid familyId, Guid creatorUserId)
    {
        var family = new Family { Id = familyId, Name = "Existing Family", Code = "EF1", CreatedBy = creatorUserId.ToString() };
        family.AddFamilyUser(creatorUserId, FamilyRole.Manager);
        return family;
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingFamilyData_AndReturnFamilyId()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        _mockUser.Setup(x => x.UserId).Returns(currentUserId);

        var existingFamily = Family.Create("Existing Family", "EXFAM", "Existing Description", "Existing Address", "Public", currentUserId);
        existingFamily.UpdateAvatar("ExistingAvatar.jpg");
        existingFamily.Id = Guid.NewGuid();
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync(CancellationToken.None);

        var originalMember1Id = Guid.NewGuid();
        var originalMember2Id = Guid.NewGuid();
        var originalRelationshipId = Guid.NewGuid();
        var originalEventId = Guid.NewGuid();

        // New: Create EventExportDto with CalendarType, SolarDate, RepeatRule, Color
        var eventToImport = new EventExportDto
        {
            Id = originalEventId,
            Name = "Imported Wedding",
            Code = "IWEDDING",
            Type = EventType.Marriage,
            Description = "DescC",
            CalendarType = CalendarType.Solar, // Corrected
            SolarDate = DateTime.Now.AddYears(-5), // Corrected
            RepeatRule = RepeatRule.Yearly, // Corrected
            Color = "#000000",
            RelatedMembers = new List<Guid> { originalMember1Id, originalMember2Id }
        };

        var familyExportDto = new FamilyExportDto
        {
            Id = Guid.NewGuid(), // This ID will be ignored by the handler, as FamilyId in command takes precedence
            Name = "Imported Family",
            Code = "IMP1",
            Description = "Description of imported family",
            Address = "Imported Address",
            AvatarUrl = "ImportedAvatar.jpg",
            Visibility = "Private",
            Members = new List<MemberExportDto>
            {
                new MemberExportDto { Id = originalMember1Id, FirstName = "Imported John", LastName = "Doe", Code = "IJOHNDOE", Nickname = "IJ", Gender = Gender.Male, DateOfBirth = DateTime.Now.AddYears(-30), PlaceOfBirth = "PlaceA", Occupation = "OccA", Biography = "BioA", AvatarUrl = "AvatarA.jpg", IsRoot = true, Order = 1 },
                new MemberExportDto { Id = originalMember2Id, FirstName = "Imported Jane", LastName = "Doe", Code = "IJANEDOE", Nickname = "IJ", Gender = Gender.Female, DateOfBirth = DateTime.Now.AddYears(-28), PlaceOfBirth = "PlaceB", Occupation = "OccB", Biography = "BioB", AvatarUrl = "AvatarB.jpg", IsRoot = false, Order = 2 }
            },
            Relationships = new List<RelationshipExportDto>
            {
                new RelationshipExportDto { Id = originalRelationshipId, SourceMemberId = originalMember1Id, TargetMemberId = originalMember2Id, Type = RelationshipType.Husband, Order = 1 }
            },
            Events = new List<EventExportDto>
            {
                eventToImport // Use the correctly initialized eventToImport
            }
        };

        // Add a dummy user to the in-memory database to satisfy FK constraint
        var user = new User(currentUserId.ToString(), "test@example.com");
        user.Id = currentUserId; // Manually set the Id
        user.UpdateProfile(currentUserId.ToString(), "test@example.com", "Test User", "Test", "User", "123456789", "avatar.jpg"); // Update profile
        _context.Users.Add(user);
        await _context.SaveChangesAsync(CancellationToken.None);
        // Authorization check is not in handler, so no setup here.

        var command = new ImportFamilyCommand { FamilyData = familyExportDto, FamilyId = existingFamily.Id, ClearExistingData = true };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue("because the import should succeed. Error: " + result.Error);
        result.Value.Should().Be(existingFamily.Id); // Should return the existing family ID

        var updatedFamily = await _context.Families
            .Include(f => f.Members)
            .Include(f => f.Relationships)
            .Include(f => f.Events)
                .ThenInclude(e => e.EventMembers)
            .FirstOrDefaultAsync(f => f.Id == existingFamily.Id);

        updatedFamily.Should().NotBeNull();
        updatedFamily!.Id.Should().Be(existingFamily.Id); // Same ID
        updatedFamily.Name.Should().Be(familyExportDto.Name);
        updatedFamily.Code.Should().Be(existingFamily.Code); // Code should not change
        updatedFamily.Visibility.Should().Be(familyExportDto.Visibility);

        updatedFamily.Members.Should().HaveCount(familyExportDto.Members.Count);
        updatedFamily.Relationships.Should().HaveCount(familyExportDto.Relationships.Count);
        updatedFamily.Events.Should().HaveCount(familyExportDto.Events.Count);

        // Verify members have new IDs and correct familyId
        foreach (var importedMember in updatedFamily.Members)
        {
            importedMember.Id.Should().NotBe(familyExportDto.Members.First(m => m.Code == importedMember.Code).Id);
            importedMember.FamilyId.Should().Be(existingFamily.Id);
        }

        // Verify relationships have new IDs and correct member IDs
        foreach (var importedRelationship in updatedFamily.Relationships)
        {
            // Find the original relationship DTO by matching properties that should be preserved
            var originalRelationshipDto = familyExportDto.Relationships.FirstOrDefault(r =>
                r.Type == importedRelationship.Type &&
                r.Order == importedRelationship.Order);

            originalRelationshipDto.Should().NotBeNull(); // Ensure we found a match

            importedRelationship.Id.Should().NotBe(originalRelationshipDto!.Id); // New ID
            importedRelationship.FamilyId.Should().Be(existingFamily.Id);

            // Ensure source/target member IDs are remapped to new member IDs
            var originalSourceMemberCode = familyExportDto.Members.First(m => m.Id == originalRelationshipDto.SourceMemberId).Code;
            var originalTargetMemberCode = familyExportDto.Members.First(m => m.Id == originalRelationshipDto.TargetMemberId).Code;

            updatedFamily.Members.Should().Contain(m => m.Id == importedRelationship.SourceMemberId && m.Code == originalSourceMemberCode);
            updatedFamily.Members.Should().Contain(m => m.Id == importedRelationship.TargetMemberId && m.Code == originalTargetMemberCode);
        }

        // Verify events have new IDs and correct member IDs
        foreach (var importedEvent in updatedFamily.Events)
        {
            var originalEventDto = familyExportDto.Events.FirstOrDefault(e =>
                e.Name == importedEvent.Name &&
                e.Code == importedEvent.Code &&
                e.Type == importedEvent.Type);

            originalEventDto.Should().NotBeNull(); // Ensure we found a match

            importedEvent.Id.Should().NotBe(originalEventDto!.Id); // New ID
            importedEvent.FamilyId.Should().Be(existingFamily.Id);
            importedEvent.EventMembers.Should().HaveCount(originalEventDto.RelatedMembers.Count);

            // Ensure related member IDs are remapped to new member IDs
            foreach (var originalRelatedMemberId in originalEventDto.RelatedMembers)
            {
                var originalRelatedMemberCode = familyExportDto.Members.First(m => m.Id == originalRelatedMemberId).Code;
                updatedFamily.Members.Should().Contain(m => importedEvent.EventMembers.Any(em => em.MemberId == m.Id) && m.Code == originalRelatedMemberCode);
            }
        }
    }



    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyExportDtoIsNull()
    {
        // Arrange
        _mockUser.Setup(x => x.UserId).Returns(Guid.NewGuid());
        // Authorization check is not in handler, so no setup here.

        var command = new ImportFamilyCommand { FamilyData = null!, FamilyId = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Family data cannot be null");
    }

    [Fact]
    public async Task Handle_ShouldSuccessfullyImportAndKeepExistingData_WhenClearExistingDataIsFalse()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var creatorUserId = Guid.NewGuid(); // Use a new GUID for creatorUserId for this test
        _mockUser.Setup(x => x.UserId).Returns(creatorUserId);

        var existingFamily = Family.Create("Existing Family for Keep", "EXFAMK", "Existing Description", "Existing Address", "Public", creatorUserId);
        existingFamily.UpdateAvatar("ExistingAvatar.jpg");
        existingFamily.Id = Guid.NewGuid();
        // Add existing member and event to the existing family so counts are meaningful
        var existingMember1 = new Member("Existing", "Member1", "EM1", existingFamily.Id, false) { Id = Guid.NewGuid() };
        existingFamily.AddMember(existingMember1); // Add to family's internal collection
        var existingEvent1 = Event.CreateSolarEvent("Existing Event 1", "EVE1", EventType.Other, DateTime.UtcNow.AddDays(-10), RepeatRule.None, existingFamily.Id);
        // Need to explicitly add the existingEvent1 to the existingFamily's _events collection for the count to be correct
        var membersField = typeof(Family).GetField("_members", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (membersField != null)
        {
            var membersCollection = membersField.GetValue(existingFamily) as HashSet<Member>;
            membersCollection?.Add(existingMember1);
        }

        var eventsField = typeof(Family).GetField("_events", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (eventsField != null)
        {
            var eventsCollection = eventsField.GetValue(existingFamily) as HashSet<Event>;
            eventsCollection?.Add(existingEvent1);
        }

        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync(CancellationToken.None);


        var originalMember1Id = Guid.NewGuid();
        var originalEventId = Guid.NewGuid();

        // New: Create EventExportDto with CalendarType, SolarDate, RepeatRule, Color
        var eventToImport = new EventExportDto
        {
            Id = originalEventId,
            Name = "Imported Event Keep",
            Code = "IEK",
            Type = EventType.Other,
            Description = "Event description keep",
            CalendarType = CalendarType.Solar, // Corrected
            SolarDate = DateTime.Now.AddYears(-5), // Corrected
            RepeatRule = RepeatRule.Yearly, // Corrected
            Color = "#FFFFFF",
            RelatedMembers = new List<Guid> { originalMember1Id }
        };

        var familyExportDto = new FamilyExportDto
        {
            Id = Guid.NewGuid(),
            Name = "Imported Family Keep",
            Code = "IMPK",
            Description = "Description of imported family keep",
            Address = "Imported Address Keep",
            AvatarUrl = "ImportedAvatarKeep.jpg",
            Visibility = "Private",
            Members = new List<MemberExportDto>
            {
                new MemberExportDto { Id = originalMember1Id, FirstName = "Imported John Keep", LastName = "Doe Keep", Code = "IJOHNDOEK", Nickname = "IJK", Gender = Gender.Male, DateOfBirth = DateTime.Now.AddYears(-30), PlaceOfBirth = "PlaceAK", Occupation = "OccAK", Biography = "BioAK", AvatarUrl = "AvatarAK.jpg", IsRoot = true, Order = 1 }
            },
            Relationships = new List<RelationshipExportDto>(), // No new relationships
            Events = new List<EventExportDto> { eventToImport } // New event
        };

        // Add a dummy user to the in-memory database to satisfy FK constraint
        var user = new User(creatorUserId.ToString(), "test_keep@example.com");
        user.Id = creatorUserId;
        user.UpdateProfile(creatorUserId.ToString(), "test_keep@example.com", "Test User Keep", "Test", "User", "123456789", "avatar.jpg");
        _context.Users.Add(user);
        await _context.SaveChangesAsync(CancellationToken.None);


        var command = new ImportFamilyCommand { FamilyData = familyExportDto, FamilyId = existingFamily.Id, ClearExistingData = false };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue("because the import should succeed. Error: " + result.Error); // Debug assertion
        result.Value.Should().Be(existingFamily.Id);

        // Verify existing data is NOT cleared
        _context.Members.Should().HaveCount(2); // 1 existing + 1 imported
        _context.Relationships.Should().HaveCount(0); // 0 existing + 0 imported
        _context.Events.Should().HaveCount(2); // 1 existing + 1 imported

        // Verify family details updated
        var updatedFamily = await _context.Families
            .Include(f => f.Members)
            .Include(f => f.Relationships)
            .Include(f => f.Events)
                .ThenInclude(e => e.EventMembers)
            .FirstOrDefaultAsync(f => f.Id == existingFamily.Id);

        updatedFamily.Should().NotBeNull();
        updatedFamily!.Id.Should().Be(existingFamily.Id);
        updatedFamily.Members.Should().HaveCount(2); // 1 existing + 1 imported
        updatedFamily.Events.Should().HaveCount(2); // 1 existing + 1 imported

        // Verify new members added
        var importedMemberA = _context.Members.FirstOrDefault(m => m.FirstName == "Imported John Keep" && m.LastName == "Doe Keep");
        importedMemberA.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRelationshipMemberIdMappingFails()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var creatorUserId = _mockUser.Object.UserId; // Use currentUserId for consistency
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
