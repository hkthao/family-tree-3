using backend.Application.Common.Models;
using backend.Application.Identity.Commands.UpdateUserProfile;

namespace backend.Application.Common.Interfaces;

public interface IAuthProvider
{
    Task<Result<AuthResult>> LoginAsync(string email, string password);
    Task<Result<AuthResult>> RegisterAsync(string email, string password, string username);
    Task<Result<AuthResult>> GetUserAsync(string userId);
    Task<Result<string>> GetAccessTokenAsync();
    Task<Result> UpdateUserProfileAsync(string userId, UpdateUserProfileCommand request);
}
