using backend.Application.Common.Exceptions;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Domain.Entities;
using FluentAssertions;
using backend.Application.Common.Interfaces;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandTests
{
    private readonly Mock<IFamilyRepository> _mockFamilyRepository;

    public DeleteFamilyCommandTests()
    {
        _mockFamilyRepository = new Mock<IFamilyRepository>();
    }

    [Fact]
    public async Task Handle_GivenValidId_ShouldDeleteFamily()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family" };
        _mockFamilyRepository.Setup(repo => repo.GetByIdAsync(familyId)).ReturnsAsync(family);
        _mockFamilyRepository.Setup(repo => repo.DeleteAsync(familyId)).Returns(Task.CompletedTask);

        var command = new DeleteFamilyCommand(familyId);
        var handler = new DeleteFamilyCommandHandler(_mockFamilyRepository.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockFamilyRepository.Verify(repo => repo.DeleteAsync(familyId), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var command = new DeleteFamilyCommand(invalidId);
        _mockFamilyRepository.Setup(repo => repo.GetByIdAsync(invalidId)).ReturnsAsync((Family)null!);
        var handler = new DeleteFamilyCommandHandler(_mockFamilyRepository.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
