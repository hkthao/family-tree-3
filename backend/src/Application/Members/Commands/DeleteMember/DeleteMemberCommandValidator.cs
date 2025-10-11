namespace backend.Application.Members.Commands.DeleteMember
{
    public class DeleteMemberCommandValidator : AbstractValidator<DeleteMemberCommand>
    {
        public DeleteMemberCommandValidator()
        {
            RuleFor(v => v.Id)
                .NotEmpty();
        }
    }
}
