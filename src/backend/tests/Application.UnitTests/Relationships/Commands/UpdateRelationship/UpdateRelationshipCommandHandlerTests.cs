using backend.Application.Relationships.Commands.UpdateRelationship;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.UpdateRelationship;

public class UpdateRelationshipCommandHandlerTests : TestBase
{
    public UpdateRelationshipCommandHandlerTests()
    {
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var family = Family.Create("Test Family", "TF", null, null, null, "Public", Guid.NewGuid());
        var sourceMember = family.AddMember(new Member("Source", "Member", "SM", family.Id));
        var targetMember = family.AddMember(new Member("Target", "Member", "TM", family.Id));
        var relationship = family.AddRelationship(sourceMember.Id, targetMember.Id, RelationshipType.Father, null);
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(x => x.CanManageFamily(family.Id)).Returns(true);

        var handler = new UpdateRelationshipCommandHandler(_context, _mockAuthorizationService.Object);
        var command = new UpdateRelationshipCommand
        {
            Id = relationship.Id,
            SourceMemberId = sourceMember.Id,
            TargetMemberId = targetMember.Id,
            Type = RelationshipType.Mother,
            FamilyId = family.Id
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        var updatedRelationship = await _context.Relationships.FindAsync(relationship.Id);
        updatedRelationship.Should().NotBeNull();
        updatedRelationship!.Type.Should().Be(RelationshipType.Mother);
        updatedRelationship.DomainEvents.Should().ContainSingle(e => e is Domain.Events.Relationships.RelationshipUpdatedEvent);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var handler = new UpdateRelationshipCommandHandler(_context, _mockAuthorizationService.Object);
        var command = new UpdateRelationshipCommand { FamilyId = familyId };

        _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId)).Returns(false);

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
        _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var handler = new UpdateRelationshipCommandHandler(_context, _mockAuthorizationService.Object);
        var command = new UpdateRelationshipCommand { FamilyId = familyId };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRelationshipNotFound()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF" });
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var handler = new UpdateRelationshipCommandHandler(_context, _mockAuthorizationService.Object);
        var command = new UpdateRelationshipCommand { Id = Guid.NewGuid(), FamilyId = familyId };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
