using backend.Application.Common.Models;

namespace backend.Application.Identity.UserProfiles.Queries.GetCurrentUserProfile;

public class GetCurrentUserProfileQuery : IRequest<Result<UserProfileDto>>
{
}
