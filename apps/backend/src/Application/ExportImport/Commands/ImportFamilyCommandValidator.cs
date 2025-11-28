namespace backend.Application.ExportImport.Commands;

public class ImportFamilyCommandValidator : AbstractValidator<ImportFamilyCommand>
{
    public ImportFamilyCommandValidator()
    {
        RuleFor(v => v.FamilyData)
            .NotNull().WithMessage("Family data cannot be null.")
            .DependentRules(() =>
            {
                RuleFor(v => v.FamilyData.Name)
                    .NotEmpty().WithMessage("Family name cannot be empty.")
                    .MaximumLength(200).WithMessage("Family name must not exceed 200 characters.");

                RuleFor(v => v.FamilyData.Code)
                    .NotEmpty().WithMessage("Family code cannot be empty.");

                RuleFor(v => v.FamilyData.Visibility)
                    .NotEmpty().WithMessage("Family visibility cannot be empty.")
                    .Must(BeAValidVisibility).WithMessage("Family visibility must be 'Public' or 'Private'.");

                RuleForEach(v => v.FamilyData.Members)
                    .SetValidator(new MemberExportDtoValidator());

                RuleForEach(v => v.FamilyData.Relationships)
                    .SetValidator(new RelationshipExportDtoValidator());

                RuleForEach(v => v.FamilyData.Events)
                    .SetValidator(new EventExportDtoValidator());
            });
    }

    private bool BeAValidVisibility(string visibility)
    {
        return visibility == "Public" || visibility == "Private";
    }
}

public class MemberExportDtoValidator : AbstractValidator<MemberExportDto>
{
    public MemberExportDtoValidator()
    {
        RuleFor(m => m.FirstName)
            .NotEmpty().WithMessage("Member first name cannot be empty.")
            .MaximumLength(100).WithMessage("Member first name must not exceed 100 characters.");

        RuleFor(m => m.LastName)
            .NotEmpty().WithMessage("Member last name cannot be empty.")
            .MaximumLength(100).WithMessage("Member last name must not exceed 100 characters.");

        RuleFor(m => m.Code)
            .NotEmpty().WithMessage("Member code cannot be empty.");

        RuleFor(m => m.Gender)
            .IsInEnum().When(m => m.Gender.HasValue).WithMessage("Invalid gender value.");
    }
}

public class RelationshipExportDtoValidator : AbstractValidator<RelationshipExportDto>
{
    public RelationshipExportDtoValidator()
    {
        RuleFor(r => r.SourceMemberId)
            .NotEmpty().WithMessage("Relationship source member ID cannot be empty.");

        RuleFor(r => r.TargetMemberId)
            .NotEmpty().WithMessage("Relationship target member ID cannot be empty.");

        RuleFor(r => r.Type)
            .IsInEnum().WithMessage("Invalid relationship type.");
    }
}

public class EventExportDtoValidator : AbstractValidator<EventExportDto>
{
    public EventExportDtoValidator()
    {
        RuleFor(e => e.Name)
            .NotEmpty().WithMessage("Event name cannot be empty.")
            .MaximumLength(200).WithMessage("Event name must not exceed 200 characters.");

        RuleFor(e => e.Code)
            .NotEmpty().WithMessage("Event code cannot be empty.");

        RuleFor(e => e.Type)
            .IsInEnum().WithMessage("Invalid event type.");
    }
}
