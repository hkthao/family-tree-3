using backend.Application.Common.Constants;
using backend.Application.MemoryItems.Commands.DeleteMemoryItem;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.MemoryItems.Commands;

public class DeleteMemoryItemCommandHandlerTests : TestBase
{
    private readonly DeleteMemoryItemCommandHandler _handler;

    public DeleteMemoryItemCommandHandlerTests()
    {
        _handler = new DeleteMemoryItemCommandHandler(_context, _mockUser.Object, _mockDateTime.Object, _mockAuthorizationService.Object);
    }

    // Helper method to create a family and a memory item for tests
    private async Task<(Family family, MemoryItem memoryItem)> CreateFamilyAndMemoryItem(Guid userId)
    {
        var testContext = _context;
        var family = Family.Create("Test Family", "TF001", null, null, "Private", userId);
        testContext.Families.Add(family);
        await testContext.SaveChangesAsync();

        var memoryItem = new MemoryItem(family.Id, "Memory to Delete", "Description");
        testContext.MemoryItems.Add(memoryItem);
        await testContext.SaveChangesAsync();

        return (family, memoryItem);
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteMemoryItem_WhenUserCanAccessFamily()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true);
        _mockDateTime.Setup(x => x.Now).Returns(new DateTime(2023, 1, 1));

        var (family, memoryItem) = await CreateFamilyAndMemoryItem(userId);

        var command = new DeleteMemoryItemCommand { Id = memoryItem.Id };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var deletedMemoryItem = await _context.MemoryItems.FindAsync(memoryItem.Id); // Use fresh context for assertion
        deletedMemoryItem.Should().NotBeNull();
        deletedMemoryItem!.IsDeleted.Should().BeTrue();
        deletedMemoryItem.DeletedBy.Should().Be(userId.ToString());
        deletedMemoryItem.DeletedDate.Should().Be(new DateTime(2023, 1, 1));
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        _mockUser.Setup(x => x.IsAuthenticated).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(false);

        var (family, memoryItem) = await CreateFamilyAndMemoryItem(Guid.NewGuid());

        var command = new DeleteMemoryItemCommand { Id = memoryItem.Id };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.Unauthorized);
        result.ErrorSource.Should().Be(ErrorSources.Authentication);

        var originalMemoryItem = await _context.MemoryItems.FindAsync(memoryItem.Id); // Use fresh context for assertion
        originalMemoryItem!.IsDeleted.Should().BeFalse(); // Should not be deleted
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

        var command = new DeleteMemoryItemCommand { Id = memoryItem.Id };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);

        var originalMemoryItem = await _context.MemoryItems.FindAsync(memoryItem.Id); // Use fresh context for assertion
        originalMemoryItem!.IsDeleted.Should().BeFalse(); // Should not be deleted
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenMemoryItemDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true);

        var command = new DeleteMemoryItemCommand { Id = Guid.NewGuid() }; // Non-existent ID

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("NotFound");
        result.ErrorSource.Should().Be("NotFound");
    }
}
