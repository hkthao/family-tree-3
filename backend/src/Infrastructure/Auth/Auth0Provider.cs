using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Infrastructure.Auth;

public class Auth0Provider : IAuthProvider
{
    private readonly List<AuthResult> _users = new();

    public Auth0Provider()
    {
        // Seed a dummy user for testing
        _users.Add(new AuthResult
        {
            UserId = "auth0|dummyuser",
            Email = "user@example.com",
            Username = "dummyuser",
            AccessToken = "dummy_access_token",
            Roles = new List<string> { "User" },
            Succeeded = true
        });
    }

    public Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = _users.FirstOrDefault(u => u.Email == email && password == "password"); // Simple password check
        if (user != null)
        {
            return Task.FromResult(new AuthResult { UserId = user.UserId, Email = user.Email, Username = user.Username, AccessToken = "new_access_token", Roles = user.Roles, Succeeded = true });
        }
        return Task.FromResult(new AuthResult { Succeeded = false, Errors = new[] { "Invalid credentials" } });
    }

    public Task<AuthResult> RegisterAsync(string email, string password, string username)
    {
        if (_users.Any(u => u.Email == email))
        {
            return Task.FromResult(new AuthResult { Succeeded = false, Errors = new[] { "Email already registered" } });
        }

        var newUser = new AuthResult
        {
            UserId = $"auth0|{Guid.NewGuid()}",
            Email = email,
            Username = username,
            AccessToken = "new_access_token",
            Roles = new List<string> { "User" },
            Succeeded = true
        };
        _users.Add(newUser);
        return Task.FromResult(newUser);
    }

    public Task<AuthResult> GetUserAsync(string userId)
    {
        var user = _users.FirstOrDefault(u => u.UserId == userId);
        if (user != null)
        {
            return Task.FromResult(new AuthResult { UserId = user.UserId, Email = user.Email, Username = user.Username, AccessToken = "current_access_token", Roles = user.Roles, Succeeded = true });
        }
        return Task.FromResult(new AuthResult { Succeeded = false, Errors = new[] { "User not found" } });
    }

    public Task<string?> GetAccessTokenAsync()
    {
        // In a real scenario, this would retrieve the current access token from a secure storage
        return Task.FromResult<string?>("current_access_token");
    }
}