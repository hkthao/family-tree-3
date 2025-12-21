using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemoryItems.Commands.UpdateMemoryItem;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.MemoryItems.Commands;

public class UpdateMemoryItemCommandHandlerTests : TestBase
{
    private readonly UpdateMemoryItemCommandHandler _handler;

    public UpdateMemoryItemCommandHandlerTests()
    {
        _handler = new UpdateMemoryItemCommandHandler(_context, _mockAuthorizationService.Object, _mockUser.Object);
    }

    // Helper method to create a family and a memory item for tests
    private async Task<(Family family, MemoryItem memoryItem)> CreateFamilyAndMemoryItem(Guid userId)
    {
        var testContext = this.GetApplicationDbContext();
        var family = Family.Create("Test Family", "TF001", null, null, "Private", userId);
        testContext.Families.Add(family);
        await testContext.SaveChangesAsync();

        var memoryItem = new MemoryItem(family.Id, "Initial Title", "Initial Description");
        testContext.MemoryItems.Add(memoryItem);
        await testContext.SaveChangesAsync();

        return (family, memoryItem);
    }

    [Fact]
    public async Task Handle_ShouldUpdateMemoryItem_WhenUserCanAccessFamily()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true);

        var (family, memoryItem) = await CreateFamilyAndMemoryItem(userId);
        var member = new Member("John", "Doe", "JD1", family.Id) { Id = Guid.NewGuid() };
        var testContext = this.GetApplicationDbContext(); // Get fresh context for this part
        testContext.Members.Add(member);
        testContext.MemoryPersons.Add(new MemoryPerson(memoryItem.Id, member.Id));
        await testContext.SaveChangesAsync();

        var command = new UpdateMemoryItemCommand
        {
            Id = memoryItem.Id,
            FamilyId = family.Id,
            Title = "Updated Title",
            Description = "Updated Description",
            HappenedAt = DateTime.Now.AddDays(-5),
            EmotionalTag = EmotionalTag.Happy,
            MemoryMedia = new[] { new UpdateMemoryMediaCommandDto { Id = Guid.NewGuid(), Url = "http://example.com/new.jpg" } },
            PersonIds = new[] { member.Id }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedMemoryItem = await this.GetApplicationDbContext().MemoryItems // Use fresh context for assertion
            .Include(mi => mi.MemoryMedia)
            .Include(mi => mi.MemoryPersons)
            .FirstOrDefaultAsync(mi => mi.Id == memoryItem.Id);

        updatedMemoryItem.Should().NotBeNull();
        updatedMemoryItem!.Title.Should().Be(command.Title);
        updatedMemoryItem.Description.Should().Be(command.Description);
        updatedMemoryItem.EmotionalTag.Should().Be(command.EmotionalTag);
        updatedMemoryItem.MemoryMedia.Should().ContainSingle(mm => mm.Url == "http://example.com/new.jpg");
        updatedMemoryItem.MemoryPersons.Should().ContainSingle(mp => mp.MemberId == member.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        _mockUser.Setup(x => x.IsAuthenticated).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(false);

        var (family, memoryItem) = await CreateFamilyAndMemoryItem(Guid.NewGuid());

        var command = new UpdateMemoryItemCommand
        {
            Id = memoryItem.Id,
            FamilyId = family.Id,
            Title = "Unauthorized Update"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.Unauthorized);
        result.ErrorSource.Should().Be(ErrorSources.Authentication);

        var originalMemoryItem = await this.GetApplicationDbContext().MemoryItems.FindAsync(memoryItem.Id); // Use fresh context for assertion
        originalMemoryItem!.Title.Should().Be("Initial Title"); // Should not be updated
    }

    [Fact]
    public async Task Handle_ShouldReturnAccessDenied_WhenUserCannotAccessFamily()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(false); // User cannot access

        var (family, memoryItem) = await CreateFamilyAndMemoryItem(Guid.NewGuid()); // Family owned by another user

        var command = new UpdateMemoryItemCommand
        {
            Id = memoryItem.Id,
            FamilyId = family.Id,
            Title = "Denied Update"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);

        var originalMemoryItem = await this.GetApplicationDbContext().MemoryItems.FindAsync(memoryItem.Id); // Use fresh context for assertion
        originalMemoryItem!.Title.Should().Be("Initial Title"); // Should not be updated
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenMemoryItemDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true);

        var (family, _) = await CreateFamilyAndMemoryItem(userId);

        var command = new UpdateMemoryItemCommand
        {
            Id = Guid.NewGuid(), // Non-existent ID
            FamilyId = family.Id,
            Title = "Non Existent"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("NotFound");
    }
}
