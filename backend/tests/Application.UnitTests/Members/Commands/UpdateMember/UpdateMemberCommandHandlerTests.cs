using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.UpdateMember;
using backend.Domain.Entities;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;
using FluentAssertions;

namespace backend.Application.UnitTests.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMongoCollection<Member>> _mockCollection;

    public UpdateMemberCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockCollection = new Mock<IMongoCollection<Member>>();

        _mockContext.Setup(c => c.Members).Returns(_mockCollection.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateMember_WhenMemberExists()
    {
        var memberId = "65e6f8a2b3c4d5e6f7a8b9c0";
        var command = new UpdateMemberCommand
        {
            Id = memberId,
            FullName = "Updated Name",
            Gender = "Female"
        };

        _mockCollection.Setup(c => c.UpdateOneAsync(
            It.IsAny<FilterDefinition<Member>>(),
            It.IsAny<UpdateDefinition<Member>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, new BsonInt64(1)));

        var handler = new UpdateMemberCommandHandler(_mockContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockCollection.Verify(c => c.UpdateOneAsync(
            It.IsAny<FilterDefinition<Member>>(),
            It.IsAny<UpdateDefinition<Member>>(),            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenMemberDoesNotExist()
    {
        // Arrange
        var memberId = "000000000000000000000000"; // Valid format, but non-existent
        var command = new UpdateMemberCommand
        {
            Id = memberId,
            FullName = "Updated Name",
            Gender = "Female"
        };

        _mockCollection.Setup(c => c.UpdateOneAsync(
            It.IsAny<FilterDefinition<Member>>(),
            It.IsAny<UpdateDefinition<Member>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateResult.Acknowledged(0, 0, null));

        var handler = new UpdateMemberCommandHandler(_mockContext.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}
