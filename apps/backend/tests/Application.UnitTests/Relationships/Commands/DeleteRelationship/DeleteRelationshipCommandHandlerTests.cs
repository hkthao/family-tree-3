
using backend.Infrastructure.Data;
using backend.Application.Common.Interfaces;
using backend.Application.Relationships.Commands.DeleteRelationship;
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // NEW
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.DeleteRelationship;

public class DeleteRelationshipCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IDateTime> _dateTimeMock;

    public DeleteRelationshipCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _dateTimeMock = new Mock<IDateTime>();
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var family = Family.Create("Test Family", "TF", null, null, "Public", Guid.NewGuid());
        var sourceMember = family.AddMember(new Member("Source", "Member", "SM", family.Id));
        var targetMember = family.AddMember(new Member("Target", "Member", "TM", family.Id));
        var relationship = family.AddRelationship(sourceMember.Id, targetMember.Id, RelationshipType.Father, null);
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(family.Id)).Returns(true);

        var handler = new DeleteRelationshipCommandHandler(_context, _authorizationServiceMock.Object, _currentUserMock.Object, _dateTimeMock.Object);
        var command = new DeleteRelationshipCommand(relationship.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        // Use a fresh context to verify the deletion
        await using var verificationContext = new ApplicationDbContext(_dbContextOptions, _mockDomainEventDispatcher.Object, _currentUserMock.Object, _dateTimeMock.Object);
        var deletedRelationship = await verificationContext.Relationships.FindAsync(relationship.Id);
        deletedRelationship.Should().BeNull();
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is Domain.Events.Relationships.RelationshipDeletedEvent)
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRelationshipNotFound()
    {
        // Arrange
        var handler = new DeleteRelationshipCommandHandler(_context, _authorizationServiceMock.Object, _currentUserMock.Object, _dateTimeMock.Object);
        var command = new DeleteRelationshipCommand(Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var relationshipId = Guid.NewGuid();

        _context.Relationships.Add(new Relationship(familyId, Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Father, 1) { Id = relationshipId });
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false);

        var handler = new DeleteRelationshipCommandHandler(_context, _authorizationServiceMock.Object, _currentUserMock.Object, _dateTimeMock.Object);
        var command = new DeleteRelationshipCommand(relationshipId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyNotFound()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var relationshipId = Guid.NewGuid();

        _context.Relationships.Add(new Relationship(familyId, Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Father, 1) { Id = relationshipId });
        // Not saving the family to the context
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var handler = new DeleteRelationshipCommandHandler(_context, _authorizationServiceMock.Object, _currentUserMock.Object, _dateTimeMock.Object);
        var command = new DeleteRelationshipCommand(relationshipId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
