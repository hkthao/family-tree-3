using backend.Application.Common.Models;
using backend.Application.UserPreferences.Queries;

namespace backend.Application.UserPreferences.Queries.GetUserPreferences;

public class GetUserPreferencesQuery : IRequest<Result<UserPreferenceDto>>
{
}
