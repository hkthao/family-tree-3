namespace backend.Application.Relationships.Commands.CreateRelationship;

public class CreateRelationshipCommandValidator : AbstractValidator<CreateRelationshipCommand>
{
    public CreateRelationshipCommandValidator()
    {
        RuleFor(v => v.SourceMemberId)
            .NotEqual(Guid.Empty).WithMessage("SourceMemberId must not be empty.");
        RuleFor(v => v.TargetMemberId)
            .NotEqual(Guid.Empty).WithMessage("TargetMemberId must not be empty.");
        RuleFor(v => v.SourceMemberId)
            .NotEqual(v => v.TargetMemberId)
            .WithMessage("SourceMemberId and TargetMemberId cannot be the same.");
    }
}
