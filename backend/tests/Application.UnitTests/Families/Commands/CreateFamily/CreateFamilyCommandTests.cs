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

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Families.Add(It.IsAny<Family>()));
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateFamilyCommandHandler(mockContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBe(Guid.Empty);
    }
}
