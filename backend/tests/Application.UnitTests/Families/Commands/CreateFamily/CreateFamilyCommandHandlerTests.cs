using backend.Application.Families.Commands.CreateFamily;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.UnitTests.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandlerTests
{
    private readonly CreateFamilyCommandHandler _handler;
    private readonly Mock<IFamilyRepository> _mockFamilyRepository;

    public CreateFamilyCommandHandlerTests()
    {
        _mockFamilyRepository = new Mock<IFamilyRepository>();
        _handler = new CreateFamilyCommandHandler(_mockFamilyRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Family_And_Return_Id()
    {
        // Arrange
        var command = new CreateFamilyCommand
        {
            Name = "Test Family",
            AvatarUrl = "logo.png",
            Description = "A long time ago..."
        };

        _mockFamilyRepository.Setup(repo => repo.AddAsync(It.IsAny<Family>()))
            .ReturnsAsync((Family family) =>
            {
                family.Id = Guid.NewGuid(); // Simulate ID generation
                return family;
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockFamilyRepository.Verify(repo => repo.AddAsync(It.Is<Family>(f => f.Name == command.Name)), Times.Once);
    }
}
