
using backend.Application.Relationships.Commands.UpdateRelationship;
using backend.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.UpdateRelationship;

public class UpdateRelationshipCommandValidatorTests
{
    private readonly UpdateRelationshipCommandValidator _validator;

    public UpdateRelationshipCommandValidatorTests()
    {
        _validator = new UpdateRelationshipCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var command = new UpdateRelationshipCommand { Id = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Should_Have_Error_When_SourceMemberId_Is_Empty()
    {
        var command = new UpdateRelationshipCommand { SourceMemberId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.SourceMemberId);
    }

    [Fact]
    public void Should_Have_Error_When_TargetMemberId_Is_Empty()
    {
        var command = new UpdateRelationshipCommand { TargetMemberId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TargetMemberId);
    }

    [Fact]
    public void Should_Have_Error_When_SourceMemberId_And_TargetMemberId_Are_The_Same()
    {
        var memberId = Guid.NewGuid();
        var command = new UpdateRelationshipCommand { SourceMemberId = memberId, TargetMemberId = memberId };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TargetMemberId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new UpdateRelationshipCommand
        {
            Id = Guid.NewGuid(),
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
