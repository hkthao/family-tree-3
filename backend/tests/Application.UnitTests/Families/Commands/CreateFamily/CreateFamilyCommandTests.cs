using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.CreateFamily;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.CreateFamily;

public class CreateFamilyCommandTests
{
    [Fact]
    public async Task ShouldCreateFamily()
    {
        var command = new CreateFamilyCommand
        {
            Name = "Test Family",
            Description = "Test Description",
            AvatarUrl = "http://test.com/avatar.jpg"
        };

        var mockFamilyRepository = new Mock<IFamilyRepository>();
        mockFamilyRepository.Setup(repo => repo.AddAsync(It.IsAny<Family>()))
            .ReturnsAsync((Family family) =>
            {
                family.Id = Guid.NewGuid();
                return family;
            });

        var handler = new CreateFamilyCommandHandler(mockFamilyRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBe(Guid.Empty);
    }
}
