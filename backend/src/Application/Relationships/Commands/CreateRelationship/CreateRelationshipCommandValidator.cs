using FluentValidation;

namespace backend.Application.Relationships.Commands.CreateRelationship;

public class CreateRelationshipCommandValidator : AbstractValidator<CreateRelationshipCommand>
{
    public CreateRelationshipCommandValidator()
    {
        RuleFor(v => v.SourceMemberId)
            .NotEmpty();

        RuleFor(v => v.TargetMemberId)
            .NotEmpty();

        RuleFor(v => v.Type)
            .NotEmpty();
    }
}
