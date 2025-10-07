namespace backend.Application.Relationships.Commands.UpdateRelationship;

public class UpdateRelationshipCommandValidator : AbstractValidator<UpdateRelationshipCommand>
{
    public UpdateRelationshipCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.SourceMemberId)
            .NotEmpty();

        RuleFor(v => v.TargetMemberId)
            .NotEmpty();

        RuleFor(v => v.Type)
            .NotEmpty();
    }
}
