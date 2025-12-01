namespace backend.Application.MemberStories.Commands.DeleteMemberStory; // Updated

public class DeleteMemberStoryCommandValidator : AbstractValidator<DeleteMemberStoryCommand> // Updated
{
    public DeleteMemberStoryCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("MemberStory ID is required."); // Updated message
    }
}
