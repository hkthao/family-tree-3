using AutoMapper;
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

    [Fact]
    public async Task Handle_ShouldUpdateExistingFamilyData_AndReturnFamilyId()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        _mockUser.Setup(x => x.UserId).Returns(currentUserId);

        var existingFamily = Family.Create("Existing Family", "EXFAM", "Existing Description", "Existing Address", "ExistingAvatar.jpg", "Public", currentUserId);
        existingFamily.Id = Guid.NewGuid();
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync(CancellationToken.None);

        var originalMember1Id = Guid.NewGuid();
        var originalMember2Id = Guid.NewGuid();
        var originalRelationshipId = Guid.NewGuid();
        var originalEventId = Guid.NewGuid();

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
                new EventExportDto { Id = originalEventId, Name = "Imported Wedding", Code = "IWEDDING", Type = EventType.Marriage, StartDate = DateTime.Now.AddYears(-5), Location = "LocationC", Description = "DescC", Color = "#000000", RelatedMembers = new List<Guid> { originalMember1Id, originalMember2Id } }
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
        result.IsSuccess.Should().BeTrue();
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
}
