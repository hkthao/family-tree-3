namespace backend.Application.FamilyMedias.Commands.DeleteFamilyMedia;

public class DeleteFamilyMediaCommandValidator : AbstractValidator<DeleteFamilyMediaCommand>
{
    public DeleteFamilyMediaCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("FamilyMedia ID is required.");
    }
}
