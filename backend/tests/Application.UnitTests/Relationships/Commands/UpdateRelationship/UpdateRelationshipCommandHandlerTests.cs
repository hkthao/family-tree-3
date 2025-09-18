using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Relationships.Commands.UpdateRelationship;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Testcontainers.MongoDb;

namespace backend.Application.UnitTests.Relationships.Commands.UpdateRelationship;

public class UpdateRelationshipCommandHandlerTests : IAsyncLifetime
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private IMongoCollection<Relationship> _relationshipsCollection = null!;
    private MongoDbContainer _mongoDbContainer = null!;

    public UpdateRelationshipCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
    }

    public async Task InitializeAsync()
    {
        _mongoDbContainer = new MongoDbBuilder().Build();
        await _mongoDbContainer.StartAsync();

        var client = new MongoClient(_mongoDbContainer.GetConnectionString());
        var database = client.GetDatabase("TestDatabase");
        _relationshipsCollection = database.GetCollection<Relationship>("Relationships");

        _mockContext.Setup(c => c.Relationships).Returns(_relationshipsCollection);
    }

    public async Task DisposeAsync()
    {
        await _mongoDbContainer.DisposeAsync();
    }

    [Fact]
    public async Task Handle_ShouldUpdateRelationship_WhenRelationshipExists()
    {
        // Arrange
        var relationshipId = "65e6f8a2b3c4d5e6f7a8b9c0";
        var existingRelationship = new Relationship
        {
            Id = relationshipId,
            SourceMemberId = MongoDB.Bson.ObjectId.Parse("65e6f8a2b3c4d5e6f7a8b9c1"),
            TargetMemberId = MongoDB.Bson.ObjectId.Parse("65e6f8a2b3c4d5e6f7a8b9c2"),
            Type = RelationshipType.Parent,
            FamilyId = MongoDB.Bson.ObjectId.Parse("65e6f8a2b3c4d5e6f7a8b9c3"),
            StartDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        await _relationshipsCollection.InsertOneAsync(existingRelationship);

        var command = new UpdateRelationshipCommand
        {
            Id = relationshipId,
            SourceMemberId = "65e6f8a2b3c4d5e6f7a8b9c4",
            TargetMemberId = "65e6f8a2b3c4d5e6f7a8b9c5",
            Type = RelationshipType.Child,
            FamilyId = "65e6f8a2b3c4d5e6f7a8b9c6",
            StartDate = new DateTime(2005, 5, 5, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2020, 10, 10, 0, 0, 0, DateTimeKind.Utc)
        };

        var handler = new UpdateRelationshipCommandHandler(_mockContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedRelationship = await _relationshipsCollection.Find(r => r.Id == relationshipId).FirstOrDefaultAsync();
        updatedRelationship.Should().NotBeNull();
        updatedRelationship!.SourceMemberId.ToString().Should().Be(command.SourceMemberId);
        updatedRelationship.TargetMemberId.ToString().Should().Be(command.TargetMemberId);
        updatedRelationship.Type.Should().Be(command.Type);
        updatedRelationship.FamilyId.ToString().Should().Be(command.FamilyId);
        updatedRelationship.StartDate.Should().Be(command.StartDate);
        updatedRelationship.EndDate.Should().Be(command.EndDate);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenRelationshipDoesNotExist()
    {
        // Arrange
        var relationshipId = "000000000000000000000000"; // Valid format, but non-existent
        var command = new UpdateRelationshipCommand
        {
            Id = relationshipId,
            SourceMemberId = "65e6f8a2b3c4d5e6f7a8b9c4",
            TargetMemberId = "65e6f8a2b3c4d5e6f7a8b9c5",
            Type = RelationshipType.Child,
            FamilyId = "65e6f8a2b3c4d5e6f7a8b9c6",
            StartDate = new DateTime(2005, 5, 5),
            EndDate = new DateTime(2020, 10, 10)
        };

        var handler = new UpdateRelationshipCommandHandler(_mockContext.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}
