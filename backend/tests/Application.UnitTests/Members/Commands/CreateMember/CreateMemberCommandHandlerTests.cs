using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.CreateMember;
using backend.Domain.Entities;
using MediatR;
using MongoDB.Driver;
using Moq;
using Xunit;
using FluentAssertions;

namespace backend.Application.UnitTests.Members.Commands.CreateMember;

public class CreateMemberCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMongoCollection<Member>> _mockCollection;

    public CreateMemberCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockCollection = new Mock<IMongoCollection<Member>>();

        _mockContext.Setup(c => c.Members).Returns(_mockCollection.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateMemberAndReturnId()
    {
        // Arrange
        var command = new CreateMemberCommand
        {
            FullName = "Test Member",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",

        };

        var handler = new CreateMemberCommandHandler(_mockContext.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockCollection.Verify(c => c.InsertOneAsync(
            It.Is<Member>(m => m.FullName == command.FullName && m.Gender == command.Gender),
            null,
            It.IsAny<CancellationToken>()),
            Times.Once);

        result.Should().NotBeNullOrEmpty();
    }
}
