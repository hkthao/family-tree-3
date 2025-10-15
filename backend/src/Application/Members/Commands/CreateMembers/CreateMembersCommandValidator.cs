using backend.Application.Members.Queries;

namespace backend.Application.Members.Commands.CreateMembers;

public class CreateMembersCommandValidator : AbstractValidator<CreateMembersCommand>
{
    public CreateMembersCommandValidator(IValidator<MemberDto> memberDtoValidator)
    {
        RuleFor(x => x.Members)
            .NotEmpty().WithMessage("At least one member is required.");

        RuleForEach(x => x.Members).SetValidator(memberDtoValidator);
    }
}