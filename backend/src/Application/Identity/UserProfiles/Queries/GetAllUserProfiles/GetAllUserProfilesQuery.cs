using backend.Application.Common.Models;

namespace backend.Application.Identity.UserProfiles.Queries.GetAllUserProfiles
{
    public record GetAllUserProfilesQuery : IRequest<Result<List<UserProfileDto>>>;
}
