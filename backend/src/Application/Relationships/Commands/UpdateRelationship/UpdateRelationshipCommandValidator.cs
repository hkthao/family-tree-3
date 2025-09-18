namespace backend.Application.Relationships.Commands.UpdateRelationship;

public class UpdateRelationshipCommandValidator : AbstractValidator<UpdateRelationshipCommand>
{
    public UpdateRelationshipCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEqual(Guid.Empty).WithMessage("Id must not be empty.");
        RuleFor(v => v.SourceMemberId)
            .NotEqual(Guid.Empty).WithMessage("SourceMemberId must not be empty.");
        RuleFor(v => v.TargetMemberId)
            .NotEqual(Guid.Empty).WithMessage("TargetMemberId must not be empty.");
        RuleFor(v => v.FamilyId)
            .NotEqual(Guid.Empty).WithMessage("FamilyId must not be empty.");
        RuleFor(v => v.Type)
            .IsInEnum();
        RuleFor(v => v.StartDate)
            .LessThanOrEqualTo(v => v.EndDate)
            .When(v => v.StartDate.HasValue && v.EndDate.HasValue)
            .WithMessage("Start date must be before or equal to End date.");
    }
}
