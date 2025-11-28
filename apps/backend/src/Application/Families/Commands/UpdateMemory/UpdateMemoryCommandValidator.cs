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
    }
}
