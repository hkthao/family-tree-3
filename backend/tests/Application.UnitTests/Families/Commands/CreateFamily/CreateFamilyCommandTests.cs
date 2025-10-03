using backend.Application.Families.Commands.CreateFamily;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

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
        var mockDbSetFamily = new Mock<DbSet<Family>>();

        mockContext.Setup(c => c.Families).Returns(mockDbSetFamily.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        Family addedFamily = null!;
        mockDbSetFamily.Setup(db => db.Add(It.IsAny<Family>()))
            .Callback<Family>(f => addedFamily = f);

        var handler = new CreateFamilyCommandHandler(mockContext.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBe(Guid.Empty);
        addedFamily.Should().NotBeNull();
        addedFamily.Name.Should().Be(command.Name);
        mockDbSetFamily.Verify(db => db.Add(It.IsAny<Family>()), Times.Once);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
