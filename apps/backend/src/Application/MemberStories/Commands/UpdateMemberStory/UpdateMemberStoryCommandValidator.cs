using backend.Domain.Enums; // NEW

namespace backend.Application.MemberStories.Commands.UpdateMemberStory; // Updated

public class UpdateMemberStoryCommandValidator : AbstractValidator<UpdateMemberStoryCommand> // Updated
{
    public UpdateMemberStoryCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("MemberStory ID is required."); // Updated message

        RuleFor(v => v.MemberId)
            .NotEmpty().WithMessage("Member ID is required.");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(120).WithMessage("Title must not exceed 120 characters.");

        RuleFor(v => v.Story)
            .NotEmpty().WithMessage("Story content is required.")
            .MaximumLength(4000).WithMessage("Story content must not exceed 4000 characters."); // Arbitrary max length

        RuleFor(v => v.StoryStyle)
            .NotNull().WithMessage("Story style is required.")
            .IsEnumName(typeof(MemberStoryStyle), caseSensitive: false)
            .WithMessage($"Invalid story style. Valid values are: {string.Join(", ", Enum.GetNames(typeof(MemberStoryStyle)))}.");

        RuleFor(v => v.Perspective)
            .NotNull().WithMessage("Perspective is required.")
            .IsEnumName(typeof(MemberStoryPerspective), caseSensitive: false)
            .WithMessage($"Invalid perspective. Valid values are: {string.Join(", ", Enum.GetNames(typeof(MemberStoryPerspective)))}.");
    }
}
