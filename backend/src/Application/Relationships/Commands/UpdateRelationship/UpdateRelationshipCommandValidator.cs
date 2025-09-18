using FluentValidation;
using backend.Domain.Enums;

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
        RuleFor(v => v.FamilyId)
            .NotEmpty();
        RuleFor(v => v.Type)
            .IsInEnum();
        RuleFor(v => v.StartDate)
            .LessThanOrEqualTo(v => v.EndDate)
            .When(v => v.StartDate.HasValue && v.EndDate.HasValue)
            .WithMessage("Start date must be before or equal to End date.");
    }
}
