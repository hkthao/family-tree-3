
using backend.Application.Common.Models;
using backend.Application.Relationships.Commands.CreateRelationship;
using backend.Application.Relationships.Commands.CreateRelationships;
using backend.Application.Relationships.Commands.Inputs;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.CreateRelationships;

public class CreateRelationshipsCommandHandlerTests
{
    private readonly Mock<IMediator> _mediatorMock;

    public CreateRelationshipsCommandHandlerTests()
    {
        _mediatorMock = new Mock<IMediator>();
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAllRelationshipsAreCreatedSuccessfully()
    {
        // Arrange
        var command = new CreateRelationshipsCommand
        {
            Relationships = new List<RelationshipInput>
            {
                new CreateRelationshipCommand { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), Type = RelationshipType.Father },
                new CreateRelationshipCommand { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), Type = RelationshipType.Mother }
            }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateRelationshipCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        var handler = new CreateRelationshipsCommandHandler(_mediatorMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenOneRelationshipCreationFails()
    {
        // Arrange
        var command = new CreateRelationshipsCommand
        {
            Relationships = new List<RelationshipInput>
            {
                new CreateRelationshipCommand { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), Type = RelationshipType.Father },
                new CreateRelationshipCommand { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), Type = RelationshipType.Mother }
            }
        };

        _mediatorMock.SetupSequence(m => m.Send(It.IsAny<CreateRelationshipCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()))
            .ReturnsAsync(Result<Guid>.Failure("Error"));

        var handler = new CreateRelationshipsCommandHandler(_mediatorMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
