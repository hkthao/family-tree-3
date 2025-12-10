using backend.Application.MemberStories.Commands.CreateMemberStory;
using backend.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;
using System;

namespace backend.Application.UnitTests.MemberStories.Commands.CreateMemberStory;

public class CreateMemberStoryCommandValidatorTests
{
    private readonly CreateMemberStoryCommandValidator _validator;

    public CreateMemberStoryCommandValidatorTests()
    {
        _validator = new CreateMemberStoryCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenMemberIdIsEmpty()
    {
        var command = new CreateMemberStoryCommand { MemberId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.MemberId)
              .WithErrorMessage("Member ID is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenTitleIsEmpty()
    {
        var command = new CreateMemberStoryCommand { MemberId = Guid.NewGuid(), Title = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenTitleExceedsMaxLength()
    {
        var command = new CreateMemberStoryCommand { MemberId = Guid.NewGuid(), Title = new string('a', 121) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Title must not exceed 120 characters.");
    }

    [Fact]
    public void ShouldHaveError_WhenStoryIsEmpty()
    {
        var command = new CreateMemberStoryCommand { MemberId = Guid.NewGuid(), Title = "Valid Title", Story = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Story)
              .WithErrorMessage("Story content is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenStoryExceedsMaxLength()
    {
        var command = new CreateMemberStoryCommand { MemberId = Guid.NewGuid(), Title = "Valid Title", Story = new string('a', 4001) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Story)
              .WithErrorMessage("Story content must not exceed 4000 characters.");
    }

    [Fact]
    public void ShouldHaveError_WhenYearIsOutOfRange()
    {
        var command = new CreateMemberStoryCommand { MemberId = Guid.NewGuid(), Title = "Valid", Story = "Valid", Year = 999 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Year)
              .WithErrorMessage($"Year must be between 1000 and {DateTime.Now.Year + 1}.");

        command = new CreateMemberStoryCommand { MemberId = Guid.NewGuid(), Title = "Valid", Story = "Valid", Year = DateTime.Now.Year + 2 };
        result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Year)
              .WithErrorMessage($"Year must be between 1000 and {DateTime.Now.Year + 1}.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenYearIsNull()
    {
        var command = new CreateMemberStoryCommand { MemberId = Guid.NewGuid(), Title = "Valid", Story = "Valid", Year = null };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Year);
    }

    [Fact]
    public void ShouldHaveError_WhenTimeRangeDescriptionExceedsMaxLength()
    {
        var command = new CreateMemberStoryCommand { MemberId = Guid.NewGuid(), Title = "Valid", Story = "Valid", TimeRangeDescription = new string('a', 101) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TimeRangeDescription)
              .WithErrorMessage("Time range description must not exceed 100 characters.");
    }

    [Fact]
    public void ShouldHaveError_WhenLifeStageIsInvalid()
    {
        var command = new CreateMemberStoryCommand { MemberId = Guid.NewGuid(), Title = "Valid", Story = "Valid", LifeStage = (LifeStage)99 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LifeStage)
              .WithErrorMessage("Invalid Life Stage.");
    }

    [Fact]
    public void ShouldHaveError_WhenLocationExceedsMaxLength()
    {
        var command = new CreateMemberStoryCommand { MemberId = Guid.NewGuid(), Title = "Valid", Story = "Valid", Location = new string('a', 201) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Location)
              .WithErrorMessage("Location must not exceed 200 characters.");
    }

    [Fact]
    public void ShouldHaveError_WhenCertaintyLevelIsInvalid()
    {
        var command = new CreateMemberStoryCommand { MemberId = Guid.NewGuid(), Title = "Valid", Story = "Valid", CertaintyLevel = (CertaintyLevel)99 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CertaintyLevel)
              .WithErrorMessage("Invalid Certainty Level.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        var command = new CreateMemberStoryCommand
        {
            MemberId = Guid.NewGuid(),
            Title = "Valid Title",
            Story = "Valid Story",
            Year = 2000,
            TimeRangeDescription = "A specific period",
            IsYearEstimated = false,
            LifeStage = LifeStage.Adulthood,
            Location = "Ho Chi Minh City",
            StorytellerId = Guid.NewGuid(),
            CertaintyLevel = CertaintyLevel.Sure
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
