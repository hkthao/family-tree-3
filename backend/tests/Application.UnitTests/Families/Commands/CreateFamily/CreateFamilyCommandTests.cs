using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.CreateFamily;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using System.Threading;
using System.Threading.Tasks;
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
        mockContext.Setup(c => c.Families.InsertOneAsync(It.IsAny<Family>(), null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateFamilyCommandHandler(mockContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<string>();
    }
}
