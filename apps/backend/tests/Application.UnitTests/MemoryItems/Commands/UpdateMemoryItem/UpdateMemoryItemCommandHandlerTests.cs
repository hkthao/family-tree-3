using backend.Application.MemoryItems.Commands.UpdateMemoryItem;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.MemoryItems.Commands.UpdateMemoryItem;

public class UpdateMemoryItemCommandHandlerTests : TestBase
{
    private readonly UpdateMemoryItemCommandHandler _handler;

    public UpdateMemoryItemCommandHandlerTests()
    {
        _handler = new UpdateMemoryItemCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldUpdateMemoryItemSuccessfully()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memoryItemId = Guid.NewGuid();
        var person1Id = Guid.NewGuid();
        var person2Id = Guid.NewGuid();
        var media1Id = Guid.NewGuid();
        var media2Id = Guid.NewGuid();

        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "FAM-TEST" });
        _context.Members.Add(new Member("Last", "Person 1", "P001", familyId) { Id = person1Id });
        _context.Members.Add(new Member("Last", "Person 2", "P002", familyId) { Id = person2Id });

        var initialMemoryItem = new MemoryItem(familyId, "Initial Title", "Initial Desc", DateTime.Now.AddDays(-20), EmotionalTag.Neutral)
        {
            Id = memoryItemId
        };
        initialMemoryItem.AddMedia(new MemoryMedia(memoryItemId, "http://oldmedia.com/1.jpg") { Id = media1Id });
        initialMemoryItem.AddMedia(new MemoryMedia(memoryItemId, "http://oldmedia.com/2.jpg") { Id = media2Id });
        initialMemoryItem.AddPerson(new MemoryPerson(memoryItemId, person1Id));

        _context.MemoryItems.Add(initialMemoryItem);
        await _context.SaveChangesAsync();

        var command = new UpdateMemoryItemCommand
        {
            Id = memoryItemId,
            FamilyId = familyId,
            Title = "Updated Title",
            Description = "Updated Description",
            HappenedAt = DateTime.Now.AddDays(-5),
            EmotionalTag = EmotionalTag.Sad,
            DeletedMediaIds = [media1Id], // Delete one media item
            MemoryMedia =
            [
                new() { Id = media2Id, Url = "http://updatedmedia.com/2.jpg" }, // Update existing media
                new() { Url = "http://newmedia.com/3.jpg" } // Add new media
            ],
            PersonIds = [person2Id] // Remove person1, add person2
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedMemoryItem = await _context.MemoryItems
            .Include(mi => mi.MemoryMedia)
            .Include(mi => mi.MemoryPersons)
            .FirstOrDefaultAsync(mi => mi.Id == memoryItemId);

        updatedMemoryItem.Should().NotBeNull();
        updatedMemoryItem!.Title.Should().Be(command.Title);
        updatedMemoryItem.Description.Should().Be(command.Description);
        updatedMemoryItem.HappenedAt.Should().Be(command.HappenedAt);
        updatedMemoryItem.EmotionalTag.Should().Be(command.EmotionalTag);

        updatedMemoryItem.MemoryMedia.Should().HaveCount(2); // media2 (updated) + new media
        updatedMemoryItem.MemoryMedia.Should().Contain(mm => mm.Url == "http://updatedmedia.com/2.jpg");
        updatedMemoryItem.MemoryMedia.Should().Contain(mm => mm.Url == "http://newmedia.com/3.jpg");
        updatedMemoryItem.MemoryMedia.Should().NotContain(mm => mm.Id == media1Id); // Check if media1 is deleted

        updatedMemoryItem.MemoryPersons.Should().HaveCount(1);
        updatedMemoryItem.MemoryPersons.Should().Contain(mp => mp.MemberId == person2Id);
        updatedMemoryItem.MemoryPersons.Should().NotContain(mp => mp.MemberId == person1Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenMemoryItemDoesNotExist()
    {
        // Arrange
        var command = new UpdateMemoryItemCommand
        {
            Id = Guid.NewGuid(), // Non-existent ID
            FamilyId = Guid.NewGuid(),
            Title = "Title",
            Description = "Desc",
            HappenedAt = DateTime.Now,
            EmotionalTag = EmotionalTag.Happy
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Not Found");
    }

    [Fact]
    public async Task Handle_ShouldUpdateMemoryItemWithoutMediaAndPersons()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memoryItemId = Guid.NewGuid();

        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "FAM-TEST" });
        await _context.SaveChangesAsync();

        var initialMemoryItem = new MemoryItem(familyId, "Initial Title", "Initial Desc", DateTime.Now.AddDays(-20), EmotionalTag.Neutral)
        {
            Id = memoryItemId
        };
        _context.MemoryItems.Add(initialMemoryItem);
        await _context.SaveChangesAsync();

        var command = new UpdateMemoryItemCommand
        {
            Id = memoryItemId,
            FamilyId = familyId,
            Title = "Updated Title (No Media/Persons)",
            Description = "Updated Description",
            HappenedAt = DateTime.Now.AddDays(-5),
            EmotionalTag = EmotionalTag.Neutral,
            DeletedMediaIds = new List<Guid>(),
            MemoryMedia = new List<UpdateMemoryMediaCommandDto>(),
            PersonIds = new List<Guid>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedMemoryItem = await _context.MemoryItems
            .Include(mi => mi.MemoryMedia)
            .Include(mi => mi.MemoryPersons)
            .FirstOrDefaultAsync(mi => mi.Id == memoryItemId);

        updatedMemoryItem.Should().NotBeNull();
        updatedMemoryItem!.Title.Should().Be(command.Title);
        updatedMemoryItem.MemoryMedia.Should().BeEmpty();
        updatedMemoryItem.MemoryPersons.Should().BeEmpty();
    }
}
