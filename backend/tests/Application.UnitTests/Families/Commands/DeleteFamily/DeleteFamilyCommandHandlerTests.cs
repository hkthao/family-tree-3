using backend.Application.Common.Exceptions;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Interfaces;

namespace backend.Application.UnitTests.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandHandlerTests
{
    private readonly DeleteFamilyCommandHandler _handler;
    private readonly Mock<IFamilyRepository> _mockFamilyRepository;

    public DeleteFamilyCommandHandlerTests()
    {
        _mockFamilyRepository = new Mock<IFamilyRepository>();
        _handler = new DeleteFamilyCommandHandler(_mockFamilyRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_Family()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family" };
        _mockFamilyRepository.Setup(repo => repo.GetByIdAsync(familyId)).ReturnsAsync(family);
        _mockFamilyRepository.Setup(repo => repo.DeleteAsync(familyId)).Returns(Task.CompletedTask);

        var command = new DeleteFamilyCommand(familyId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockFamilyRepository.Verify(repo => repo.DeleteAsync(familyId), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Family_Does_Not_Exist()
    {
        // Arrange
        var command = new DeleteFamilyCommand(Guid.NewGuid());
        _mockFamilyRepository.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync((Family?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
