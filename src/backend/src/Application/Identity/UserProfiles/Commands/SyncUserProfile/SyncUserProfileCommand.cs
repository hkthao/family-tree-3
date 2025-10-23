using System.Security.Claims;
using backend.Application.Common.Models;

namespace backend.Application.Identity.UserProfiles.Commands.SyncUserProfile;

public class SyncUserProfileCommand : IRequest<Result<bool>>
{
    public ClaimsPrincipal UserPrincipal { get; set; } = null!;
}
