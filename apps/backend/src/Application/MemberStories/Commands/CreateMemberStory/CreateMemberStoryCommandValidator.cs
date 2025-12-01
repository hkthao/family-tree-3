using backend.Domain.Enums; // NEW

namespace backend.Application.MemberStories.Commands.CreateMemberStory; // Updated

public class CreateMemberStoryCommandValidator : AbstractValidator<CreateMemberStoryCommand> // Updated
{
    public CreateMemberStoryCommandValidator()
    {
        RuleFor(v => v.MemberId)
            .NotEmpty().WithMessage("Member ID is required.");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(120).WithMessage("Title must not exceed 120 characters.");

        RuleFor(v => v.Story)
            .NotEmpty().WithMessage("Story content is required.")
            .MaximumLength(4000).WithMessage("Story content must not exceed 4000 characters."); // Arbitrary max length

        RuleFor(v => v.RawInput)
            .MaximumLength(4000).WithMessage("Raw input must not exceed 4000 characters."); // Arbitrary max length

        RuleFor(v => v.StoryStyle)
            .IsEnumName(typeof(MemberStoryStyle), caseSensitive: false)
            .WithMessage($"Invalid story style. Valid values are: {string.Join(", ", Enum.GetNames(typeof(MemberStoryStyle)))}.")
            .Unless(v => string.IsNullOrEmpty(v.StoryStyle));

        RuleFor(v => v.Perspective)
            .IsEnumName(typeof(MemberStoryPerspective), caseSensitive: false)
            .WithMessage($"Invalid perspective. Valid values are: {string.Join(", ", Enum.GetNames(typeof(MemberStoryPerspective)))}.")
            .Unless(v => string.IsNullOrEmpty(v.Perspective));
    }
}
