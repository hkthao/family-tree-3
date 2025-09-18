using backend.Application.Relationships.Commands.CreateRelationship;
using FluentValidation.TestHelper;
using System;
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
        var command = new CreateRelationshipCommand { SourceMemberId = Guid.Empty, TargetMemberId = Guid.NewGuid(), FamilyId = Guid.NewGuid(), StartDate = DateTime.Now, EndDate = DateTime.Now };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.SourceMemberId);
    }

    [Fact]
    public void Should_Have_Error_When_TargetMemberId_Is_Empty()
    {
        var command = new CreateRelationshipCommand { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.Empty, FamilyId = Guid.NewGuid(), StartDate = DateTime.Now, EndDate = DateTime.Now };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TargetMemberId);
    }

    [Fact]
    public void Should_Have_Error_When_SourceMemberId_And_TargetMemberId_Are_Same()
    {
        var SourceMemberId = Guid.NewGuid();
        var command = new CreateRelationshipCommand { SourceMemberId = SourceMemberId, TargetMemberId = SourceMemberId, FamilyId = Guid.NewGuid(), StartDate = DateTime.Now, EndDate = DateTime.Now };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.SourceMemberId)
              .WithErrorMessage("SourceMemberId and TargetMemberId cannot be the same.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new CreateRelationshipCommand { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), FamilyId = Guid.NewGuid(), StartDate = DateTime.Now, EndDate = DateTime.Now };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}