using backend.Application.Relationships.Commands.CreateRelationship;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.CreateRelationship;

public class CreateRelationshipCommandHandlerTests : TestBase
{
    public CreateRelationshipCommandHandlerTests()
    {
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var family = Family.Create("Test Family", "TF", null, null, null, "Public", Guid.NewGuid());
        var sourceMember = family.AddMember(new Member("Source", "Member", "SM", family.Id));
        var targetMember = family.AddMember(new Member("Target", "Member", "TM", family.Id));
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(x => x.CanManageFamily(family.Id)).Returns(true);

        var handler = new CreateRelationshipCommandHandler(_context, _mockAuthorizationService.Object);
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = sourceMember.Id,
            TargetMemberId = targetMember.Id,
            Type = RelationshipType.Father,
            FamilyId = family.Id
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        var relationship = await _context.Relationships.FindAsync(result.Value);
        relationship.Should().NotBeNull();
        family.DomainEvents.Should().ContainSingle(e => e is Domain.Events.Relationships.RelationshipCreatedEvent);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSourceMemberNotFound()
    {
        // Arrange
        var handler = new CreateRelationshipCommandHandler(_context, _mockAuthorizationService.Object);
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father,
            FamilyId = Guid.NewGuid()
        };

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
        var sourceMemberId = Guid.NewGuid();
        var targetMemberId = Guid.NewGuid();

        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF" });
        _context.Members.AddRange(
            new Member("Source", "Member", "SM", familyId) { Id = sourceMemberId },
            new Member("Target", "Member", "TM", familyId) { Id = targetMemberId }
        );
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId)).Returns(false);

        var handler = new CreateRelationshipCommandHandler(_context, _mockAuthorizationService.Object);
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = sourceMemberId,
            TargetMemberId = targetMemberId,
            Type = RelationshipType.Father,
            FamilyId = familyId
        };

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
        var sourceMemberId = Guid.NewGuid();

        _context.Members.Add(new Member("Source", "Member", "SM", familyId) { Id = sourceMemberId });
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var handler = new CreateRelationshipCommandHandler(_context, _mockAuthorizationService.Object);
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = sourceMemberId,
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father,
            FamilyId = familyId
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
