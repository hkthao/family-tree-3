namespace backend.Application.FamilyMedia.Commands.DeleteFamilyMedia;

public class DeleteFamilyMediaCommandValidator : AbstractValidator<DeleteFamilyMediaCommand>
{
    public DeleteFamilyMediaCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("FamilyMedia ID is required.");

        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");
    }
}
