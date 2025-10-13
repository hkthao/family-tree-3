namespace backend.Application.Relationships.Commands.CreateRelationship;

public class CreateRelationshipCommandValidator : AbstractValidator<CreateRelationshipCommand>
{
    public CreateRelationshipCommandValidator()
    {
        RuleFor(v => v.SourceMemberId)
            .NotEmpty().WithMessage("SourceMemberId cannot be empty.");

        RuleFor(v => v.TargetMemberId)
            .NotEmpty().WithMessage("TargetMemberId cannot be empty.")
            .NotEqual(v => v.SourceMemberId).WithMessage("SourceMemberId and TargetMemberId cannot be the same.");

        RuleFor(v => v.Type)
            .IsInEnum().WithMessage("Invalid RelationshipType value.");
    }
}