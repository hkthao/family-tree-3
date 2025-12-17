using backend.Application.Common.Mappings;
using backend.Application.Common.Models;
using backend.Application.MemoryItems.DTOs;
using backend.Application.MemoryItems.Queries.GetMemoryItemDetail;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.MemoryItems.Queries.GetMemoryItemDetail;

public class GetMemoryItemDetailQueryHandlerTests : TestBase
{
    private readonly GetMemoryItemDetailQueryHandler _handler;

    public GetMemoryItemDetailQueryHandlerTests()
    {
        _handler = new GetMemoryItemDetailQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnMemoryItemDetail_WhenMemoryItemExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId1 = Guid.NewGuid();
        var memberId2 = Guid.NewGuid();

        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "FAM-TEST" });
        _context.Members.Add(new Member("Last", "Member 1", "M001", familyId) { Id = memberId1 });
        _context.Members.Add(new Member("Last", "Member 2", "M002", familyId) { Id = memberId2 });
        await _context.SaveChangesAsync();

        var memoryItem = new MemoryItem(familyId, "Test Memory", "Description", DateTime.Now.AddDays(-10), EmotionalTag.Happy);
        memoryItem.AddMedia(new MemoryMedia(memoryItem.Id, "http://example.com/media.jpg"));
        memoryItem.AddPerson(new MemoryPerson(memoryItem.Id, memberId1));
        _context.MemoryItems.Add(memoryItem);
        await _context.SaveChangesAsync();

        var query = new GetMemoryItemDetailQuery { Id = memoryItem.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(memoryItem.Id);
        result.Value.Title.Should().Be(memoryItem.Title);
        result.Value.MemoryMedia.Should().HaveCount(1);
        result.Value.MemoryPersons.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenMemoryItemDoesNotExist()
    {
        // Arrange
        var query = new GetMemoryItemDetailQuery { Id = Guid.NewGuid() }; // Non-existent ID

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Not Found");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenMemoryItemIsSoftDeleted()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "FAM-TEST" });
        await _context.SaveChangesAsync();

        var softDeletedMemoryItem = new MemoryItem(familyId, "Soft Deleted Memory", "Desc", DateTime.Now.AddDays(-5), EmotionalTag.Neutral)
        {
            IsDeleted = true,
            DeletedDate = DateTime.Now,
            DeletedBy = Guid.NewGuid().ToString()
        };
        _context.MemoryItems.Add(softDeletedMemoryItem);
        await _context.SaveChangesAsync();

        var query = new GetMemoryItemDetailQuery { Id = softDeletedMemoryItem.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Not Found");
    }
}
