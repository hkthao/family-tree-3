using FluentAssertions;
using NSubstitute;
using Xunit;

// Giả định các namespace và class này đã tồn tồn tại trong dự án của bạn
// using FamilyTree.Application.Common.Interfaces;
// using FamilyTree.Application.Relationships.Commands;
// using FamilyTree.Domain.Entities;
// using FamilyTree.Domain.Enums;

namespace Application.UnitTests.Relationships.Commands.CreateRelationship;

public class CreateRelationshipCommandValidatorTests
{
    private readonly IRelationshipRepository _relationshipRepository;
    private readonly CreateRelationshipCommandValidator _validator;

    public CreateRelationshipCommandValidatorTests()
    {
        _relationshipRepository = Substitute.For<IRelationshipRepository>();
        _validator = new CreateRelationshipCommandValidator(_relationshipRepository);
    }

    [Fact]
    public void Should_Have_Error_When_SourceMemberId_Is_Empty()
    {
        var command = new CreateRelationshipCommand { SourceMemberId = string.Empty, TargetMemberId = "member2" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.SourceMemberId);
    }

    [Fact]
    public void Should_Have_Error_When_TargetMemberId_Is_Empty()
    {
        var command = new CreateRelationshipCommand { SourceMemberId = "member1", TargetMemberId = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TargetMemberId);
    }

    /// <summary>
    /// Covers Test Case: TC_REL_04
    /// </summary>
    [Fact]
    public async Task Should_Have_Error_When_Source_And_Target_Are_Same()
    {
        var command = new CreateRelationshipCommand { SourceMemberId = "member1", TargetMemberId = "member1" };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.TargetMemberId)
              .WithErrorMessage("Không thể tạo mối quan hệ với chính mình.");
    }

    /// <summary>
    /// Covers Test Case: TC_REL_05
    /// </summary>
    [Fact]
    public async Task Should_Have_Error_When_Relationship_Already_Exists()
    {
        // Arrange
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = "member1",
            TargetMemberId = "member2",
            Type = RelationshipType.SPOUSE_OF
        };
        _relationshipRepository.RelationshipExistsAsync(command.SourceMemberId, command.TargetMemberId, command.Type, Arg.Any<CancellationToken>())
                               .Returns(true);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Relationship")
              .WithErrorMessage("Mối quan hệ này đã tồn tại.");
    }

    /// <summary>
    /// Covers Test Case: TC_REL_06
    /// </summary>
    [Fact]
    public async Task Should_Have_Error_When_Adding_Third_Parent()
    {
        // Arrange
        var childId = "child1";
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = "newParent",
            TargetMemberId = childId,
            Type = RelationshipType.PARENT_OF
        };

        var existingParents = new List<Relationship>
        {
            new() { SourceMemberId = "parent1", TargetMemberId = childId, Type = RelationshipType.PARENT_OF },
            new() { SourceMemberId = "parent2", TargetMemberId = childId, Type = RelationshipType.PARENT_OF }
        };

        _relationshipRepository.GetParentsAsync(childId, Arg.Any<CancellationToken>())
                               .Returns(existingParents);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Relationship")
              .WithErrorMessage("Thành viên đã có đủ 2 cha/mẹ.");
    }

    /// <summary>
    /// Covers Test Case: TC_REL_09
    /// </summary>
    [Fact]
    public async Task Should_Have_Error_When_Adding_Second_Spouse()
    {
        // Arrange
        var memberId = "member1";
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = memberId,
            TargetMemberId = "newSpouse",
            Type = RelationshipType.SPOUSE_OF
        };
        _relationshipRepository.HasSpouseAsync(memberId, Arg.Any<CancellationToken>())
                               .Returns(true);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Relationship")
              .WithErrorMessage("Thành viên đã có vợ/chồng.");
    }

    /// <summary>
    /// Covers Test Cases: TC_REL_07, TC_REL_08
    /// </summary>
    [Fact]
    public async Task Should_Have_Error_On_Circular_Dependency()
    {
        // Arrange: Trying to make an ancestor (source) a child of a descendant (target)
        // which means the source is a parent of the target.
        var descendantId = "descendant";
        var ancestorId = "ancestor";
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = descendantId, // The new parent
            TargetMemberId = ancestorId,   // The new child
            Type = RelationshipType.PARENT_OF
        };

        // Mock that the new child (ancestorId) is an ancestor of the new parent (descendantId)
        _relationshipRepository.IsAncestorAsync(ancestorId, descendantId, Arg.Any<CancellationToken>())
                               .Returns(true);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Relationship")
              .WithErrorMessage("Không thể tạo mối quan hệ vòng lặp (tổ tiên không thể là con cháu của chính mình).");
    }

    [Fact]
    public async Task Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = "member1",
            TargetMemberId = "member2",
            Type = RelationshipType.PARENT_OF
        };

        _relationshipRepository.RelationshipExistsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RelationshipType>(), Arg.Any<CancellationToken>()).Returns(false);
        _relationshipRepository.GetParentsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new List<Relationship>());
        _relationshipRepository.IsAncestorAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}