using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.UpdateFamily;
using backend.Domain.Entities;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly UpdateFamilyCommandHandler _handler;

    public UpdateFamilyCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new UpdateFamilyCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Family()
    {
        // Arrange
        var family = new Family { Name = "Old Name" };
        var command = new UpdateFamilyCommand
        {
            Id = "1",
            Name = "New Name",
            Description = "New Desc"
        };

        var cursor = new Mock<IAsyncCursor<Family>>();
        cursor.Setup(_ => _.Current).Returns(new List<Family> { family });
        cursor
            .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        cursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Returns(Task.FromResult(false));

        var collectionMock = new Mock<IMongoCollection<Family>>();
        collectionMock.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<Family>>(),
            It.IsAny<FindOptions<Family, Family>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursor.Object);

        _contextMock.Setup(x => x.Families).Returns(collectionMock.Object);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        collectionMock.Verify(x => x.ReplaceOneAsync(
            It.IsAny<FilterDefinition<Family>>(),
            It.Is<Family>(f => f.Name == command.Name),
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Family_Does_Not_Exist()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = "1" };

        var cursor = new Mock<IAsyncCursor<Family>>();
        cursor.Setup(_ => _.Current).Returns(new List<Family>());
        cursor
            .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(false);
        cursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(false));

        var collectionMock = new Mock<IMongoCollection<Family>>();
        collectionMock.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<Family>>(),
            It.IsAny<FindOptions<Family, Family>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursor.Object);

        _contextMock.Setup(x => x.Families).Returns(collectionMock.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
