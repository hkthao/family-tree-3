using backend.Application.Common.Exceptions;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<DbSet<Family>> _mockDbSetFamily;

    public DeleteFamilyCommandTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockDbSetFamily = new Mock<DbSet<Family>>();

        _mockContext.Setup(c => c.Families).Returns(_mockDbSetFamily.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_GivenValidId_ShouldDeleteFamily()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family" };

        _mockDbSetFamily.Setup(db => db.FindAsync(familyId)).ReturnsAsync(family);
        _mockDbSetFamily.Setup(db => db.Remove(It.IsAny<Family>()));

        var command = new DeleteFamilyCommand(familyId);
        var handler = new DeleteFamilyCommandHandler(_mockContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockDbSetFamily.Verify(db => db.Remove(It.Is<Family>(f => f.Id == familyId)), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        _mockDbSetFamily.Setup(db => db.FindAsync(invalidId)).ReturnsAsync((Family?)null);

        var command = new DeleteFamilyCommand(invalidId);
        var handler = new DeleteFamilyCommandHandler(_mockContext.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
