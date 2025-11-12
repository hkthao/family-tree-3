
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.UserActivities.Commands.RecordActivity;

public class RecordActivityCommandValidatorTests
{
    private readonly RecordActivityCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var command = new RecordActivityCommand { UserId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_UserId_Is_Specified()
    {
        var command = new RecordActivityCommand { UserId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Should_Have_Error_When_ActivitySummary_Is_Null()
    {
        var command = new RecordActivityCommand { ActivitySummary = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ActivitySummary);
    }

    [Fact]
    public void Should_Have_Error_When_ActivitySummary_Is_Empty()
    {
        var command = new RecordActivityCommand { ActivitySummary = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ActivitySummary);
    }

    [Fact]
    public void Should_Not_Have_Error_When_ActivitySummary_Is_Specified()
    {
        var command = new RecordActivityCommand { ActivitySummary = "Test" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.ActivitySummary);
    }

    [Fact]
    public void Should_Have_Error_When_ActionType_Is_Invalid()
    {
        var command = new RecordActivityCommand { ActionType = (UserActionType)999 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ActionType);
    }

    [Fact]
    public void Should_Not_Have_Error_When_ActionType_Is_Valid()
    {
        var command = new RecordActivityCommand { ActionType = UserActionType.Login };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.ActionType);
    }

    [Fact]
    public void Should_Have_Error_When_TargetType_Is_Invalid()
    {
        var command = new RecordActivityCommand { TargetType = (TargetType)999 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TargetType);
    }

    [Fact]
    public void Should_Not_Have_Error_When_TargetType_Is_Valid()
    {
        var command = new RecordActivityCommand { TargetType = TargetType.Family };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.TargetType);
    }
}
