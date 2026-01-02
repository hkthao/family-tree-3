using backend.Application.Common.Interfaces;

namespace backend.Application.VoiceProfiles.Queries.GetVoiceProfilesByMemberId;

/// <summary>
/// Validator cho GetVoiceProfilesByMemberIdQuery.
/// </summary>
public class GetVoiceProfilesByMemberIdQueryValidator : AbstractValidator<GetVoiceProfilesByMemberIdQuery>
{
    private readonly IApplicationDbContext _context;

    public GetVoiceProfilesByMemberIdQueryValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.MemberId)
            .NotEmpty().WithMessage("Member ID không được để trống.")
            .MustAsync(BeExistingMember).WithMessage("Member không tồn tại.");
    }

    private async Task<bool> BeExistingMember(Guid memberId, CancellationToken cancellationToken)
    {
        return await _context.Members.AnyAsync(m => m.Id == memberId, cancellationToken);
    }
}
