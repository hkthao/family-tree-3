using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface IAuthProvider
{
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RegisterAsync(string email, string password, string username);
    Task<AuthResult> GetUserAsync(string userId);
    Task<string?> GetAccessTokenAsync();
}