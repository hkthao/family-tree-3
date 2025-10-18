using backend.Application.Relationships.Commands.CreateRelationship;
using backend.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.CreateRelationship;

public class CreateRelationshipCommandValidatorTests
{
    private readonly CreateRelationshipCommandValidator _validator;

    public CreateRelationshipCommandValidatorTests()
    {
        _validator = new CreateRelationshipCommandValidator();
    }

    [Fact]
    public void ShouldNotHaveValidationErrorForValidCommand()
    {
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenSourceMemberIdIsEmpty()
    {
        var command = new CreateRelationshipCommand
        {
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
        var command = new CreateRelationshipCommand
        {
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
        var command = new CreateRelationshipCommand
        {
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
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = (RelationshipType)999 // Invalid enum value
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }
}
