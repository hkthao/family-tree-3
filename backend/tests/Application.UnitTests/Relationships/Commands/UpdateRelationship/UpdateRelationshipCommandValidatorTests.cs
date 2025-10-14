using FluentValidation.TestHelper;
using Xunit;
using backend.Application.Relationships.Commands.UpdateRelationship;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Relationships.Commands.UpdateRelationship;

public class UpdateRelationshipCommandValidatorTests
{
    private readonly UpdateRelationshipCommandValidator _validator;

    public UpdateRelationshipCommandValidatorTests()
    {
        _validator = new UpdateRelationshipCommandValidator();
    }

    [Fact]
    public void ShouldNotHaveValidationErrorForValidCommand()
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

    [Fact]
    public void ShouldHaveValidationErrorWhenIdIsEmpty()
    {
        var command = new UpdateRelationshipCommand
        {
            Id = Guid.Empty,
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenSourceMemberIdIsEmpty()
    {
        var command = new UpdateRelationshipCommand
        {
            Id = Guid.NewGuid(),
            SourceMemberId = Guid.Empty,
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.SourceMemberId);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenTargetMemberIdIsEmpty()
    {
        var command = new UpdateRelationshipCommand
        {
            Id = Guid.NewGuid(),
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.Empty,
            Type = RelationshipType.Father
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TargetMemberId);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenSourceAndTargetMemberIdsAreTheSame()
    {
        var memberId = Guid.NewGuid();
        var command = new UpdateRelationshipCommand
        {
            Id = Guid.NewGuid(),
            SourceMemberId = memberId,
            TargetMemberId = memberId,
            Type = RelationshipType.Father
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TargetMemberId);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenTypeIsInvalid()
    {
        var command = new UpdateRelationshipCommand
        {
            Id = Guid.NewGuid(),
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = (RelationshipType)999 // Invalid enum value
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }
}
