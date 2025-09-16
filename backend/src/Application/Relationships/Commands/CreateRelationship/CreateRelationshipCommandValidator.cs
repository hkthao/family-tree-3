using FluentValidation;

namespace backend.Application.Relationships.Commands.CreateRelationship;

public class CreateRelationshipCommandValidator : AbstractValidator<CreateRelationshipCommand>
{
    public CreateRelationshipCommandValidator()
    {
        RuleFor(v => v.MemberId)
            .NotEmpty();
        RuleFor(v => v.TargetId)
            .NotEmpty();
    }
}
