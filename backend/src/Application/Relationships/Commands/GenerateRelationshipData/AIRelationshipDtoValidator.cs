namespace backend.Application.Relationships.Commands.GenerateRelationshipData;

public class AIRelationshipDtoValidator : AbstractValidator<AIRelationshipDto>
{
    public AIRelationshipDtoValidator()
    {
        RuleFor(x => x.SourceMemberName)
            .NotEmpty().WithMessage("Source member name is required.");

        RuleFor(x => x.TargetMemberName)
            .NotEmpty().WithMessage("Target member name is required.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid relationship type.");

        // Add more validation rules as needed
    }
}