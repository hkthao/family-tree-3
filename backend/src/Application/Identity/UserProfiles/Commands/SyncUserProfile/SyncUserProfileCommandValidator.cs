using FluentValidation;
using System.Security.Claims;

namespace backend.Application.Identity.UserProfiles.Commands.SyncUserProfile;

public class SyncUserProfileCommandValidator : AbstractValidator<SyncUserProfileCommand>
{
    public SyncUserProfileCommandValidator()
    {
        RuleFor(v => v.UserPrincipal)
            .NotNull().WithMessage("UserPrincipal cannot be null.");
    }
}
