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
        var familyId = Guid.NewGuid();
        var relationshipId = Guid.NewGuid();

        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF" };
        var sourceMember = family.CreateMember("Source", "Member", "SM");
        var targetMember = family.CreateMember("Target", "Member", "TM");
        family.AddMember(sourceMember);
        family.AddMember(targetMember);
        var relationship = new Relationship(familyId, sourceMember.Id, targetMember.Id, RelationshipType.Father) { Id = relationshipId };
        _context.Families.Add(family);
        _context.Relationships.Add(relationship);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var handler = new UpdateRelationshipCommandHandler(_context, _mockAuthorizationService.Object);
        var command = new UpdateRelationshipCommand
        {
            Id = relationshipId,
            SourceMemberId = sourceMember.Id,
            TargetMemberId = targetMember.Id,
            Type = RelationshipType.Mother,
            FamilyId = familyId
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
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
