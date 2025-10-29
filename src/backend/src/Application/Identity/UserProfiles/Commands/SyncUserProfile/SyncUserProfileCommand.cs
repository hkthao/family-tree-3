using System.Security.Claims;
using backend.Application.Common.Models;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Domain.Entities;

namespace backend.Application.Identity.UserProfiles.Commands.SyncUserProfile;

public class SyncUserProfileCommand : IRequest<Result<UserProfileDto>>
{
    public ClaimsPrincipal UserPrincipal { get; set; } = null!;
}
