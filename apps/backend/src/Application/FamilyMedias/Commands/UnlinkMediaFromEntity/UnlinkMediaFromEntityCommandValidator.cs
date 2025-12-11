namespace backend.Application.FamilyMedias.Commands.UnlinkMediaFromEntity;

public class UnlinkMediaFromEntityCommandValidator : AbstractValidator<UnlinkMediaFromEntityCommand>
{
    public UnlinkMediaFromEntityCommandValidator()
    {
        RuleFor(v => v.FamilyMediaId)
            .NotEmpty().WithMessage("FamilyMedia ID is required.");

        RuleFor(v => v.RefType)
            .IsInEnum().WithMessage("Invalid reference type.");

        RuleFor(v => v.RefId)
            .NotEmpty().WithMessage("Reference ID is required.");
    }
}
