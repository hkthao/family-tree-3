namespace backend.Application.Members.Commands.UpdateMemberRelationships;

public class UpdateMemberRelationshipsCommandValidator : AbstractValidator<UpdateMemberRelationshipsCommand>
{
    public UpdateMemberRelationshipsCommandValidator()
    {
        RuleFor(v => v.MemberId)
            .NotEmpty().WithMessage("MemberId không được để trống.");

        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("FamilyId không được để trống.");

        // FatherId, MotherId, HusbandId, WifeId are nullable, so Empty is not required.
        // If they were not nullable, we would add .NotEmpty()
    }
}
