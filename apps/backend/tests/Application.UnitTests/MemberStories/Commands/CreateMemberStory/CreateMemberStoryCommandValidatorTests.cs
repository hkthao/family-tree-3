using backend.Application.MemberStories.Commands.CreateMemberStory;
using backend.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

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
    public void ShouldHaveError_WhenStoryStyleIsInvalid()
    {
        var command = new CreateMemberStoryCommand { MemberId = Guid.NewGuid(), Title = "Valid Title", Story = "Valid Story", StoryStyle = "InvalidStyle" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.StoryStyle)
              .WithErrorMessage($"Invalid story style. Valid values are: {string.Join(", ", Enum.GetNames(typeof(MemberStoryStyle)))}.");
    }



    [Fact]
    public void ShouldHaveError_WhenPerspectiveIsInvalid()
    {
        var command = new CreateMemberStoryCommand { MemberId = Guid.NewGuid(), Title = "Valid Title", Story = "Valid Story", StoryStyle = MemberStoryStyle.Nostalgic.ToString(), Perspective = "InvalidPerspective" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Perspective)
              .WithErrorMessage($"Invalid perspective. Valid values are: {string.Join(", ", Enum.GetNames(typeof(MemberStoryPerspective)))}.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        var command = new CreateMemberStoryCommand
        {
            MemberId = Guid.NewGuid(),
            Title = "Valid Title",
            Story = "Valid Story",
            StoryStyle = MemberStoryStyle.Nostalgic.ToString(),
            Perspective = MemberStoryPerspective.FirstPerson.ToString(),
            OriginalImageUrl = "http://example.com/original.jpg",
            ResizedImageUrl = "http://example.com/resized.jpg",
            RawInput = "Some raw input",
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
