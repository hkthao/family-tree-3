using backend.Application.Common.Exceptions;
using backend.Application.Families.Commands.UpdateFamily;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Interfaces;

namespace backend.Application.UnitTests.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandlerTests
{
    private readonly UpdateFamilyCommandHandler _handler;
    private readonly Mock<IFamilyRepository> _mockFamilyRepository;

    public UpdateFamilyCommandHandlerTests()
    {
        _mockFamilyRepository = new Mock<IFamilyRepository>();
        _handler = new UpdateFamilyCommandHandler(_mockFamilyRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Family()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Old Name" };
        _mockFamilyRepository.Setup(repo => repo.GetByIdAsync(familyId)).ReturnsAsync(family);
        _mockFamilyRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Family>())).Returns(Task.CompletedTask);

        var command = new UpdateFamilyCommand
        {
            Id = familyId,
            Name = "New Name",
            Description = "New Desc"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockFamilyRepository.Verify(repo => repo.UpdateAsync(It.Is<Family>(f => f.Id == familyId && f.Name == command.Name && f.Description == command.Description)), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Family_Does_Not_Exist()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid() };
        _mockFamilyRepository.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync(null as Family);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
