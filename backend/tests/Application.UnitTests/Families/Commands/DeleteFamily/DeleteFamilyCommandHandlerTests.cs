using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Domain.Entities;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly DeleteFamilyCommandHandler _handler;

    public DeleteFamilyCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new DeleteFamilyCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_Family()
    {
        // Arrange
        var command = new DeleteFamilyCommand("1");
        var deleteResult = new Mock<DeleteResult>();
        deleteResult.Setup(r => r.DeletedCount).Returns(1);

        var collectionMock = new Mock<IMongoCollection<Family>>();
        collectionMock.Setup(x => x.DeleteOneAsync(
            It.IsAny<FilterDefinition<Family>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult.Object);

        _contextMock.Setup(x => x.Families).Returns(collectionMock.Object);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        collectionMock.Verify(x => x.DeleteOneAsync(
            It.IsAny<FilterDefinition<Family>>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Family_Does_Not_Exist()
    {
        // Arrange
        var command = new DeleteFamilyCommand("1");
        var deleteResult = new Mock<DeleteResult>();
        deleteResult.Setup(r => r.DeletedCount).Returns(0);

        var collectionMock = new Mock<IMongoCollection<Family>>();
        collectionMock.Setup(x => x.DeleteOneAsync(
            It.IsAny<FilterDefinition<Family>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult.Object);

        _contextMock.Setup(x => x.Families).Returns(collectionMock.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
