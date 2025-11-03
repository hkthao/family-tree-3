
using backend.Application.Relationships.Commands.CreateRelationship;
using backend.Application.Relationships.Commands.CreateRelationships;
using backend.Application.Relationships.Commands.Inputs;
using backend.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.CreateRelationships;

public class CreateRelationshipsCommandValidatorTests
{
    private readonly CreateRelationshipsCommandValidator _validator;

    public CreateRelationshipsCommandValidatorTests()
    {
        _validator = new CreateRelationshipsCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Relationships_List_Is_Empty()
    {
        var command = new CreateRelationshipsCommand { Relationships = new List<RelationshipInput>() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Relationships);
    }

    [Fact]
    public void Should_Have_Error_When_SourceMemberId_Is_Empty_In_A_Relationship()
    {
        var command = new CreateRelationshipsCommand
        {
            Relationships = new List<RelationshipInput>
            {
                new CreateRelationshipCommand { SourceMemberId = Guid.Empty, TargetMemberId = Guid.NewGuid(), Type = RelationshipType.Father }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Relationships[0].SourceMemberId");
    }

    [Fact]
    public void Should_Have_Error_When_TargetMemberId_Is_Empty_In_A_Relationship()
    {
        var command = new CreateRelationshipsCommand
        {
            Relationships = new List<RelationshipInput>
            {
                new CreateRelationshipCommand { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.Empty, Type = RelationshipType.Father }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Relationships[0].TargetMemberId");
    }

    [Fact]
    public void Should_Have_Error_When_SourceMemberId_And_TargetMemberId_Are_The_Same_In_A_Relationship()
    {
        var memberId = Guid.NewGuid();
        var command = new CreateRelationshipsCommand
        {
            Relationships = new List<RelationshipInput>
            {
                new CreateRelationshipCommand { SourceMemberId = memberId, TargetMemberId = memberId, Type = RelationshipType.Father }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Relationships[0]");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new CreateRelationshipsCommand
        {
            Relationships = new List<RelationshipInput>
            {
                new CreateRelationshipCommand { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), Type = RelationshipType.Father }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
