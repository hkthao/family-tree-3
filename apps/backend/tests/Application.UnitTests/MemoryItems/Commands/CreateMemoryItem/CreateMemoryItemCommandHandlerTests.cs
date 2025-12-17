using backend.Application.Common.Models;
using backend.Application.MemoryItems.Commands.CreateMemoryItem;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.MemoryItems.Commands.CreateMemoryItem;

public class CreateMemoryItemCommandHandlerTests : TestBase
{
    private readonly CreateMemoryItemCommandHandler _handler;

    public CreateMemoryItemCommandHandlerTests()
    {
        _handler = new CreateMemoryItemCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldCreateMemoryItem_WhenFamilyExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "FAM-TEST" });
        await _context.SaveChangesAsync();

        var personId1 = Guid.NewGuid();
        var personId2 = Guid.NewGuid();
        _context.Members.Add(new Member("Last", "Person 1", "P001", familyId) { Id = personId1 });
        _context.Members.Add(new Member("Last", "Person 2", "P002", familyId) { Id = personId2 });
        await _context.SaveChangesAsync();


        var command = new CreateMemoryItemCommand
        {
            FamilyId = familyId,
            Title = "Test Memory Item",
            Description = "A description for the test memory item.",
            HappenedAt = DateTime.Now.AddDays(-10),
            EmotionalTag = EmotionalTag.Happy,
            Media = new List<CreateMemoryMediaCommandDto>
            {
                new() { Id = Guid.NewGuid(), Url = "http://example.com/photo1.jpg" },
                new() { Id = Guid.NewGuid(), Url = "http://example.com/photo2.jpg" }
            },
            PersonIds = new List<Guid> { personId1, personId2 }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var createdMemoryItem = await _context.MemoryItems
            .Include(mi => mi.MemoryMedia)
            .Include(mi => mi.MemoryPersons)
            .FirstOrDefaultAsync(mi => mi.Id == result.Value);

        createdMemoryItem.Should().NotBeNull();
        createdMemoryItem!.FamilyId.Should().Be(command.FamilyId);
        createdMemoryItem.Title.Should().Be(command.Title);
        createdMemoryItem.Description.Should().Be(command.Description);
        createdMemoryItem.HappenedAt.Should().Be(command.HappenedAt);
        createdMemoryItem.EmotionalTag.Should().Be(command.EmotionalTag);
        createdMemoryItem.MemoryMedia.Should().HaveCount(2);
        createdMemoryItem.MemoryMedia.Should().Contain(mm => mm.Url == "http://example.com/photo1.jpg");
        createdMemoryItem.MemoryPersons.Should().HaveCount(2);
        createdMemoryItem.MemoryPersons.Should().Contain(mp => mp.MemberId == personId1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyDoesNotExist()
    {
        // Arrange
        var command = new CreateMemoryItemCommand
        {
            FamilyId = Guid.NewGuid(), // Non-existent family ID
            Title = "Test Memory Item",
            Description = "A description for the test memory item.",
            HappenedAt = DateTime.Now.AddDays(-10),
            EmotionalTag = EmotionalTag.Happy
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Family not found.");
    }

    [Fact]
    public async Task Handle_ShouldCreateMemoryItemWithoutMediaAndPersons()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "FAM-TEST" });
        await _context.SaveChangesAsync();

        var command = new CreateMemoryItemCommand
        {
            FamilyId = familyId,
            Title = "Memory Item without Media and Persons",
            Description = "This memory item has no media or associated persons.",
            HappenedAt = DateTime.Now.AddDays(-5),
            EmotionalTag = EmotionalTag.Neutral,
            Media = new List<CreateMemoryMediaCommandDto>(), // Empty media
            PersonIds = new List<Guid>() // Empty persons
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var createdMemoryItem = await _context.MemoryItems
            .Include(mi => mi.MemoryMedia)
            .Include(mi => mi.MemoryPersons)
            .FirstOrDefaultAsync(mi => mi.Id == result.Value);

        createdMemoryItem.Should().NotBeNull();
        createdMemoryItem!.MemoryMedia.Should().BeEmpty();
        createdMemoryItem.MemoryPersons.Should().BeEmpty();
    }
}
