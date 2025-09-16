using backend.Application.Common.Models;
using MongoDB.Bson;

namespace backend.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(ObjectId userId);

    Task<bool> IsInRoleAsync(ObjectId userId, string role);

    Task<bool> AuthorizeAsync(ObjectId userId, string policyName);

    Task<(Result Result, ObjectId UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(ObjectId userId);
}
