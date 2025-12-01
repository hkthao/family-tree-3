using backend.Application.MemberStories.Commands.UpdateMemberStory;
using backend.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.MemberStories.Commands.UpdateMemberStory;

public class UpdateMemberStoryCommandValidatorTests
{
    private readonly UpdateMemberStoryCommandValidator _validator;

    public UpdateMemberStoryCommandValidatorTests()
    {
        _validator = new UpdateMemberStoryCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenIdIsEmpty()
    {
        var command = new UpdateMemberStoryCommand { Id = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("MemberStory ID is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenMemberIdIsEmpty()
    {
        var command = new UpdateMemberStoryCommand { Id = Guid.NewGuid(), MemberId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.MemberId)
              .WithErrorMessage("Member ID is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenTitleIsEmpty()
    {
        var command = new UpdateMemberStoryCommand { Id = Guid.NewGuid(), MemberId = Guid.NewGuid(), Title = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenTitleExceedsMaxLength()
    {
        var command = new UpdateMemberStoryCommand { Id = Guid.NewGuid(), MemberId = Guid.NewGuid(), Title = new string('a', 121) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Title must not exceed 120 characters.");
    }

    [Fact]
    public void ShouldHaveError_WhenStoryIsEmpty()
    {
        var command = new UpdateMemberStoryCommand { Id = Guid.NewGuid(), MemberId = Guid.NewGuid(), Title = "Valid Title", Story = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Story)
              .WithErrorMessage("Story content is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenStoryExceedsMaxLength()
    {
        var command = new UpdateMemberStoryCommand { Id = Guid.NewGuid(), MemberId = Guid.NewGuid(), Title = "Valid Title", Story = new string('a', 4001) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Story)
              .WithErrorMessage("Story content must not exceed 4000 characters.");
    }

    [Fact]
    public void ShouldHaveError_WhenStoryStyleIsNull()
    {
        var command = new UpdateMemberStoryCommand { Id = Guid.NewGuid(), MemberId = Guid.NewGuid(), Title = "Valid Title", Story = "Valid Story", StoryStyle = null };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.StoryStyle)
              .WithErrorMessage("Story style is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenStoryStyleIsInvalid()
    {
        var command = new UpdateMemberStoryCommand { Id = Guid.NewGuid(), MemberId = Guid.NewGuid(), Title = "Valid Title", Story = "Valid Story", StoryStyle = "InvalidStyle" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.StoryStyle)
              .WithErrorMessage($"Invalid story style. Valid values are: {string.Join(", ", Enum.GetNames(typeof(MemberStoryStyle)))}.");
    }

    [Fact]
    public void ShouldHaveError_WhenPerspectiveIsNull()
    {
        var command = new UpdateMemberStoryCommand { Id = Guid.NewGuid(), MemberId = Guid.NewGuid(), Title = "Valid Title", Story = "Valid Story", StoryStyle = MemberStoryStyle.Nostalgic.ToString(), Perspective = null };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Perspective)
              .WithErrorMessage("Perspective is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenPerspectiveIsInvalid()
    {
        var command = new UpdateMemberStoryCommand { Id = Guid.NewGuid(), MemberId = Guid.NewGuid(), Title = "Valid Title", Story = "Valid Story", StoryStyle = MemberStoryStyle.Nostalgic.ToString(), Perspective = "InvalidPerspective" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Perspective)
              .WithErrorMessage($"Invalid perspective. Valid values are: {string.Join(", ", Enum.GetNames(typeof(MemberStoryPerspective)))}.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        var command = new UpdateMemberStoryCommand
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            Title = "Valid Title",
            Story = "Valid Story",
            StoryStyle = MemberStoryStyle.Nostalgic.ToString(),
            Perspective = MemberStoryPerspective.FirstPerson.ToString()
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
