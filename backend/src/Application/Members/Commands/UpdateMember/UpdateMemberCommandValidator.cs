namespace backend.Application.Members.Commands.UpdateMember
{
    public class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
    {
        public UpdateMemberCommandValidator()
        {
            RuleFor(v => v.Id)
                .NotEmpty();

            RuleFor(v => v.FirstName)
                .MaximumLength(100)
                .NotEmpty();

            RuleFor(v => v.LastName)
                .MaximumLength(100)
                .NotEmpty();

            RuleFor(v => v.FamilyId)
                .NotEmpty();
        }
    }
}
