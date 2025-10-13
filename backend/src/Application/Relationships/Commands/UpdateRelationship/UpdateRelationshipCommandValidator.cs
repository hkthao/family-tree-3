using FluentValidation;
using backend.Domain.Enums;

namespace backend.Application.Relationships.Commands.UpdateRelationship;

public class UpdateRelationshipCommandValidator : AbstractValidator<UpdateRelationshipCommand>
{
    public UpdateRelationshipCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id cannot be empty.");

        RuleFor(v => v.SourceMemberId)
            .NotEmpty().WithMessage("SourceMemberId cannot be empty.");

        RuleFor(v => v.TargetMemberId)
            .NotEmpty().WithMessage("TargetMemberId cannot be empty.")
            .NotEqual(v => v.SourceMemberId).WithMessage("SourceMemberId and TargetMemberId cannot be the same.");

        RuleFor(v => v.Type)
            .IsInEnum().WithMessage("Invalid RelationshipType value.");
    }
}