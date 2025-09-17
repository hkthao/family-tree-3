using Xunit;
using Moq;
using backend.Application.Common.Interfaces;
using backend.Application.Relationships.Commands.CreateRelationship;
using System.Threading;
using System.Threading.Tasks;
using backend.Domain.Entities;
using FluentAssertions;
using backend.Domain.Enums;
using backend.Application.Common.Exceptions;
using FluentValidation;
using MongoDB.Driver;

namespace backend.tests.Application.UnitTests.Relationships;

public class RelationshipServiceTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;

    public RelationshipServiceTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _contextMock.Setup(x => x.Relationships).Returns(new Mock<IMongoCollection<Relationship>>().Object);
    }

    [Fact]
    public async Task CreateRelationship_ShouldCreateRelationship_WhenRequestIsValid()
    {
        // Arrange
        var command = new CreateRelationshipCommand { MemberId = "60d5ec49e04a4a5c8c8b4567", TargetId = "60d5ec49e04a4a5c8c8b4568", Type = RelationshipType.Parent, FamilyId = "60d5ec49e04a4a5c8c8b4569", StartDate = DateTime.Now, EndDate = DateTime.Now };
        var handler = new CreateRelationshipCommandHandler(_contextMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(x => x.Relationships.InsertOneAsync(It.IsAny<Relationship>(), null, CancellationToken.None), Times.Once);
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateRelationship_ShouldThrowValidationException_WhenMemberIdAndTargetIdAreSame()
    {
        // Arrange
        var command = new CreateRelationshipCommand { MemberId = "60d5ec49e04a4a5c8c8b4567", TargetId = "60d5ec49e04a4a5c8c8b4567", Type = RelationshipType.Parent, FamilyId = "60d5ec49e04a4a5c8c8b4569", StartDate = DateTime.Now, EndDate = DateTime.Now };
        var validator = new CreateRelationshipCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
