using backend.Domain.Enums;

namespace backend.Application.MemberStories.Commands.UpdateMemberStory;

public class UpdateMemberStoryCommandValidator : AbstractValidator<UpdateMemberStoryCommand>
{
    public UpdateMemberStoryCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("MemberStory ID is required.");

        RuleFor(v => v.MemberId)
            .NotEmpty().WithMessage("Member ID is required.");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(120).WithMessage("Title must not exceed 120 characters.");

        RuleFor(v => v.Story)
            .NotEmpty().WithMessage("Story content is required.")
            .MaximumLength(4000).WithMessage("Story content must not exceed 4000 characters."); // Arbitrary max length

        RuleFor(v => v.Year)
            .InclusiveBetween(1000, DateTime.Now.Year + 1).When(v => v.Year.HasValue)
            .WithMessage($"Year must be between 1000 and {DateTime.Now.Year + 1}.");

        RuleFor(v => v.TimeRangeDescription)
            .MaximumLength(100).WithMessage("Time range description must not exceed 100 characters.");

        RuleFor(v => v.LifeStage)
            .IsInEnum().WithMessage("Invalid Life Stage.");

        RuleFor(v => v.Location)
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters.");

        RuleFor(v => v.CertaintyLevel)
            .IsInEnum().WithMessage("Invalid Certainty Level.");
    }
}
