using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<Result> DeleteUserAsync(string userId);
}
