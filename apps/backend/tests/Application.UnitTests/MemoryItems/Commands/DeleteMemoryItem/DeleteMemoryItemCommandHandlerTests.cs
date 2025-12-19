using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemoryItems.Commands.DeleteMemoryItem;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.MemoryItems.Commands.DeleteMemoryItem;

public class DeleteMemoryItemCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly DeleteMemoryItemCommandHandler _handler;

    public DeleteMemoryItemCommandHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _dateTimeMock = new Mock<IDateTime>();
        _handler = new DeleteMemoryItemCommandHandler(_context, _currentUserMock.Object, _dateTimeMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteMemoryItemSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var memoryItemId = Guid.NewGuid();
        var deletionTime = DateTime.Now;

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _dateTimeMock.Setup(x => x.Now).Returns(deletionTime);

        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "FAM-TEST" });
        var memoryItem = new MemoryItem(familyId, "Memory to delete", "Desc", DateTime.Now, Domain.Enums.EmotionalTag.Neutral)
        {
            Id = memoryItemId
        };
        _context.MemoryItems.Add(memoryItem);
        await _context.SaveChangesAsync();

        var command = new DeleteMemoryItemCommand { Id = memoryItemId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deletedMemoryItem = await _context.MemoryItems.FindAsync(memoryItemId);
        deletedMemoryItem.Should().NotBeNull();
        deletedMemoryItem!.IsDeleted.Should().BeTrue();
        deletedMemoryItem.DeletedDate.Should().Be(deletionTime);
        deletedMemoryItem.DeletedBy.Should().Be(userId.ToString());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenMemoryItemDoesNotExist()
    {
        // Arrange
        var command = new DeleteMemoryItemCommand { Id = Guid.NewGuid() }; // Non-existent ID

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Not Found");
    }
}
