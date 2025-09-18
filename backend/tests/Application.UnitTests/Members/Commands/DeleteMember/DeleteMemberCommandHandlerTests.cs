using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.DeleteMember;
using backend.Domain.Entities;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;
using FluentAssertions;

namespace backend.Application.UnitTests.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMongoCollection<Member>> _mockCollection;

    public DeleteMemberCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockCollection = new Mock<IMongoCollection<Member>>();

        _mockContext.Setup(c => c.Members).Returns(_mockCollection.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteMember_WhenMemberExists()
    {
        var memberId = "65e6f8a2b3c4d5e6f7a8b9c0";
        var command = new DeleteMemberCommand(memberId);

        _mockCollection.Setup(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<Member>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteResult.Acknowledged(1));

        var handler = new DeleteMemberCommandHandler(_mockContext.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockCollection.Verify(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<Member>>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenMemberDoesNotExist()
    {
        // Arrange
        var memberId = "000000000000000000000000"; // Valid format, but non-existent
        var command = new DeleteMemberCommand(memberId);

        _mockCollection.Setup(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<Member>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteResult.Acknowledged(0));

        var handler = new DeleteMemberCommandHandler(_mockContext.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}
