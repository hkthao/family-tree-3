using FluentValidation.TestHelper;
using Xunit;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using System;

namespace backend.Application.UnitTests.UserActivities.Commands.RecordActivity;

public class RecordActivityCommandValidatorTests
{
    private readonly RecordActivityCommandValidator _validator;

    public RecordActivityCommandValidatorTests()
    {
        _validator = new RecordActivityCommandValidator();
    }

    [Fact]
    public void ShouldNotHaveValidationErrorForValidCommand()
    {
        var command = new RecordActivityCommand
        {
            UserProfileId = Guid.NewGuid(),
            ActionType = UserActionType.CreateFamily,
            TargetType = TargetType.Family,
            TargetId = Guid.NewGuid().ToString(),
            ActivitySummary = "Created a new family."
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenUserProfileIdIsEmpty()
    {
        var command = new RecordActivityCommand { UserProfileId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserProfileId);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenActionTypeIsInvalid()
    {
        var command = new RecordActivityCommand { ActionType = (UserActionType)999 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ActionType);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenTargetTypeIsInvalid()
    {
        var command = new RecordActivityCommand { TargetType = (TargetType)999 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TargetType);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenActivitySummaryIsNull()
    {
        var command = new RecordActivityCommand { ActivitySummary = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ActivitySummary);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenActivitySummaryIsEmpty()
    {
        var command = new RecordActivityCommand { ActivitySummary = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ActivitySummary);
    }
}
