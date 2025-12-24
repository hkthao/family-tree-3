using backend.Application.Common.Constants;
using backend.Application.MemoryItems.Commands.CreateMemoryItem;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.MemoryItems.Commands;

public class CreateMemoryItemCommandHandlerTests : TestBase
{
    private readonly CreateMemoryItemCommandHandler _handler;

    public CreateMemoryItemCommandHandlerTests()
    {
        _handler = new CreateMemoryItemCommandHandler(_context, _mockAuthorizationService.Object, _mockUser.Object);
    }

    // Helper method to create a family and a member for tests
    private async Task<(Family family, Member member)> CreateFamilyAndMember(Guid userId)
    {
        var family = Family.Create("Test Family", "TF001", null, null, "Private", userId);
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var member = new Member("Test", "Member", "TM001", family.Id);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        return (family, member);
    }

    [Fact]
    public async Task Handle_ShouldCreateMemoryItem_WhenUserCanAccessFamily()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true);

        var (family, member) = await CreateFamilyAndMember(userId);

        var command = new CreateMemoryItemCommand
        {
            FamilyId = family.Id,
            Title = "My First Memory",
            Description = "A lovely day",
            HappenedAt = DateTime.Now.AddDays(-10),
            EmotionalTag = EmotionalTag.Happy,
            MemoryMedia = new[] { new CreateMemoryMediaCommandDto { Url = "http://example.com/pic1.jpg" } },
            PersonIds = new[] { member.Id }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var memoryItem = await this.GetApplicationDbContext().MemoryItems // Use fresh context for assertion
            .Include(mi => mi.MemoryMedia)
            .Include(mi => mi.MemoryPersons)
            .FirstOrDefaultAsync(mi => mi.Id == result.Value);

        memoryItem.Should().NotBeNull();
        memoryItem!.Title.Should().Be(command.Title);
        memoryItem.MemoryMedia.Should().ContainSingle(mm => mm.Url == "http://example.com/pic1.jpg");
        memoryItem.MemoryPersons.Should().ContainSingle(mp => mp.MemberId == member.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        _mockUser.Setup(x => x.IsAuthenticated).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(false);

        var (family, member) = await CreateFamilyAndMember(Guid.NewGuid()); // Family owned by another user

        var command = new CreateMemoryItemCommand
        {
            FamilyId = family.Id,
            Title = "Unauthorized Memory"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.Unauthorized);
        result.ErrorSource.Should().Be(ErrorSources.Authentication);
        this.GetApplicationDbContext().MemoryItems.Should().BeEmpty(); // Use fresh context for assertion
    }

    [Fact]
    public async Task Handle_ShouldReturnAccessDenied_WhenUserCannotAccessFamily()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(false); // User cannot access

        var (family, member) = await CreateFamilyAndMember(Guid.NewGuid()); // Family owned by another user

        var command = new CreateMemoryItemCommand
        {
            FamilyId = family.Id,
            Title = "Denied Memory"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        this.GetApplicationDbContext().MemoryItems.Should().BeEmpty(); // Use fresh context for assertion
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true);

        var nonExistentFamilyId = Guid.NewGuid();
        var command = new CreateMemoryItemCommand
        {
            FamilyId = nonExistentFamilyId,
            Title = "Non Existent Family Memory"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.FamilyNotFound, nonExistentFamilyId));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
        this.GetApplicationDbContext().MemoryItems.Should().BeEmpty(); // Use fresh context for assertion
    }
}
