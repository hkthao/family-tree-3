
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
    public void Should_Have_Error_When_SourceMemberId_Is_Empty()
    {
        var command = new CreateRelationshipCommand { SourceMemberId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.SourceMemberId);
    }

    [Fact]
    public void Should_Have_Error_When_TargetMemberId_Is_Empty()
    {
        var command = new CreateRelationshipCommand { TargetMemberId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TargetMemberId);
    }

    [Fact]
    public void Should_Have_Error_When_SourceMemberId_And_TargetMemberId_Are_The_Same()
    {
        var memberId = Guid.NewGuid();
        var command = new CreateRelationshipCommand { SourceMemberId = memberId, TargetMemberId = memberId };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TargetMemberId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
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
}
