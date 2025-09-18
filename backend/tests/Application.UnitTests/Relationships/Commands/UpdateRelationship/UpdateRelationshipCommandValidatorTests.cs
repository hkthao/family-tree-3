using FluentValidation.TestHelper;
using backend.Application.Relationships.Commands.UpdateRelationship;
using backend.Domain.Enums;
using System;
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
        var command = new UpdateRelationshipCommand { Id = Guid.Empty, SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), FamilyId = Guid.NewGuid(), Type = RelationshipType.Parent };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Should_Have_Error_When_SourceMemberId_Is_Empty()
    {
        var command = new UpdateRelationshipCommand { Id = Guid.NewGuid(), SourceMemberId = Guid.Empty, TargetMemberId = Guid.NewGuid(), FamilyId = Guid.NewGuid(), Type = RelationshipType.Parent };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.SourceMemberId);
    }

    [Fact]
    public void Should_Have_Error_When_TargetMemberId_Is_Empty()
    {
        var command = new UpdateRelationshipCommand { Id = Guid.NewGuid(), SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.Empty, FamilyId = Guid.NewGuid(), Type = RelationshipType.Parent };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TargetMemberId);
    }

    [Fact]
    public void Should_Have_Error_When_FamilyId_Is_Empty()
    {
        var command = new UpdateRelationshipCommand { Id = Guid.NewGuid(), SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), FamilyId = Guid.Empty, Type = RelationshipType.Parent };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FamilyId);
    }

    [Fact]
    public void Should_Have_Error_When_Type_Is_Invalid()
    {
        var command = new UpdateRelationshipCommand { Id = Guid.NewGuid(), SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), FamilyId = Guid.NewGuid(), Type = (RelationshipType)99 }; // Invalid enum value
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void Should_Have_Error_When_StartDate_Is_After_EndDate()
    {
        var command = new UpdateRelationshipCommand { Id = Guid.NewGuid(), SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), FamilyId = Guid.NewGuid(), Type = RelationshipType.Parent, StartDate = new DateTime(2020, 1, 1), EndDate = new DateTime(2019, 1, 1) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.StartDate)
              .WithErrorMessage("Start date must be before or equal to End date.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new UpdateRelationshipCommand { Id = Guid.NewGuid(), SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), FamilyId = Guid.NewGuid(), Type = RelationshipType.Parent, StartDate = new DateTime(2019, 1, 1), EndDate = new DateTime(2020, 1, 1) };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}