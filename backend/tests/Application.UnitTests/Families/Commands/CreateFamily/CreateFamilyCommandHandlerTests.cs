using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.CreateFamily;
using backend.Domain.Entities;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateFamilyCommandHandler _handler;

    public CreateFamilyCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateFamilyCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Family_And_Return_Id()
    {
        // Arrange
        var command = new CreateFamilyCommand
        {
            Name = "Test Family",
            Address = "123 Test St",
            LogoUrl = "logo.png",
            Description = "A long time ago..."
        };

        var collectionMock = new Mock<IMongoCollection<Family>>();
        _contextMock.Setup(x => x.Families).Returns(collectionMock.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNullOrEmpty();
        collectionMock.Verify(x => x.InsertOneAsync(
            It.Is<Family>(f => f.Name == command.Name),
            null,
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
