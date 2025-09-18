using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Relationships.Commands.DeleteRelationship;
using backend.Domain.Entities;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.DeleteRelationship;

public class DeleteRelationshipCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMongoCollection<Relationship>> _mockCollection;

    public DeleteRelationshipCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockCollection = new Mock<IMongoCollection<Relationship>>();

        _mockContext.Setup(c => c.Relationships).Returns(_mockCollection.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteRelationship_WhenRelationshipExists()
    {
        // Arrange
        var relationshipId = "65e6f8a2b3c4d5e6f7a8b9c0";
        var command = new DeleteRelationshipCommand(relationshipId);

        _mockCollection.Setup(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<Relationship>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteResult.Acknowledged(1));

        var handler = new DeleteRelationshipCommandHandler(_mockContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockCollection.Verify(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<Relationship>>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenRelationshipDoesNotExist()
    {
        // Arrange
        var relationshipId = "000000000000000000000000"; // Valid format, but non-existent
        var command = new DeleteRelationshipCommand(relationshipId);

        _mockCollection.Setup(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<Relationship>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteResult.Acknowledged(0));

        var handler = new DeleteRelationshipCommandHandler(_mockContext.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}
