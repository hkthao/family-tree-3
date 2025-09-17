using backend.Application.Relationships.Commands.CreateRelationship;
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
    public void Should_Have_Error_When_MemberId_Is_Empty()
    {
        var command = new CreateRelationshipCommand { MemberId = string.Empty, TargetId = "member2", FamilyId = "family1", StartDate = DateTime.Now, EndDate = DateTime.Now };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.MemberId);
    }

    [Fact]
    public void Should_Have_Error_When_TargetId_Is_Empty()
    {
        var command = new CreateRelationshipCommand { MemberId = "member1", TargetId = string.Empty, FamilyId = "family1", StartDate = DateTime.Now, EndDate = DateTime.Now };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TargetId);
    }

    [Fact]
    public void Should_Have_Error_When_MemberId_And_TargetId_Are_Same()
    {
        var command = new CreateRelationshipCommand { MemberId = "member1", TargetId = "member1", FamilyId = "family1", StartDate = DateTime.Now, EndDate = DateTime.Now };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.MemberId)
              .WithErrorMessage("MemberId and TargetId cannot be the same.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new CreateRelationshipCommand { MemberId = "member1", TargetId = "member2", FamilyId = "family1", StartDate = DateTime.Now, EndDate = DateTime.Now };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
