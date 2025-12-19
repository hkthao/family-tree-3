namespace backend.Application.MemoryItems.Commands.CreateMemoryItem;

public class CreateMemoryItemCommandValidator : AbstractValidator<CreateMemoryItemCommand>
{
    public CreateMemoryItemCommandValidator()
    {
        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("FamilyId is required.");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(v => v.HappenedAt)
            .LessThanOrEqualTo(DateTime.Now).WithMessage("HappenedAt cannot be in the future.");

        RuleFor(v => v.EmotionalTag)
            .IsInEnum().WithMessage("Invalid EmotionalTag value.");

        RuleFor(v => v.PersonIds)
            .NotNull().WithMessage("PersonIds cannot be null.")
            .ForEach(x => x.NotEmpty().WithMessage("Each PersonId must not be empty."));
    }
}
