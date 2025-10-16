namespace backend.Application.Relationships.Commands.GenerateRelationshipData;

public class GenerateRelationshipDataCommandValidator : AbstractValidator<GenerateRelationshipDataCommand>
{
    public GenerateRelationshipDataCommandValidator()
    {
        RuleFor(v => v.Prompt)
            .NotEmpty().WithMessage("Prompt is required.")
            .MaximumLength(1000).WithMessage("Prompt must not exceed 1000 characters.");
    }
}
