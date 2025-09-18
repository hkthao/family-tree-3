using backend.Application.Common.Interfaces;
using backend.Application.Relationships.Commands.CreateRelationship;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
namespace backend.Application.UnitTests.Relationships.Commands.CreateRelationship;

public class CreateRelationshipCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMongoCollection<Relationship>> _mockCollection;

    public CreateRelationshipCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockCollection = new Mock<IMongoCollection<Relationship>>();

        _mockContext.Setup(c => c.Relationships).Returns(_mockCollection.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateRelationshipAndReturnId()
    {
        // Arrange
        var command = new CreateRelationshipCommand
        {
            MemberId = "65e6f8a2b3c4d5e6f7a8b9c0",
            Type = RelationshipType.Parent,
            TargetId = "65e6f8a2b3c4d5e6f7a8b9c1",
            FamilyId = "65e6f8a2b3c4d5e6f7a8b9c2",
            StartDate = new DateTime(2000, 1, 1)
        };

        var handler = new CreateRelationshipCommandHandler(_mockContext.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockCollection.Verify(c => c.InsertOneAsync(
            It.Is<Relationship>(r => r.SourceMemberId.ToString() == command.MemberId && r.Type == command.Type && r.TargetMemberId.ToString() == command.TargetId),
            null,
            It.IsAny<CancellationToken>()),
            Times.Once);

        result.Should().NotBeNullOrEmpty();
    }
}
