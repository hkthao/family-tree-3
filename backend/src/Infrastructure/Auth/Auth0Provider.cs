using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Identity.Commands.UpdateUserProfile;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Auth;

public class Auth0Provider : IAuthProvider
{
    private readonly List<AuthResult> _users = new();
    private readonly ILogger<Auth0Provider> _logger;

    public Auth0Provider(ILogger<Auth0Provider> logger)
    {
        _logger = logger;
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

    public Task<Result<AuthResult>> LoginAsync(string email, string password)
    {
        const string source = "Auth0Provider.LoginAsync";
        try
        {
            var user = _users.FirstOrDefault(u => u.Email == email && password == "password"); // Simple password check
            if (user != null)
            {
                return Task.FromResult(Result<AuthResult>.Success(new AuthResult { UserId = user.UserId, Email = user.Email, Username = user.Username, AccessToken = "new_access_token", Roles = user.Roles, Succeeded = true }));
            }
            return Task.FromResult(Result<AuthResult>.Failure("Invalid credentials", errorSource: source));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for email {Email}", source, email);
            return Task.FromResult(Result<AuthResult>.Failure(ex.Message, errorSource: source));
        }
    }

    public Task<Result<AuthResult>> RegisterAsync(string email, string password, string username)
    {
        const string source = "Auth0Provider.RegisterAsync";
        try
        {
            if (_users.Any(u => u.Email == email))
            {
                return Task.FromResult(Result<AuthResult>.Failure("Email already registered", errorSource: source));
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
            return Task.FromResult(Result<AuthResult>.Success(newUser));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for email {Email}", source, email);
            return Task.FromResult(Result<AuthResult>.Failure(ex.Message, errorSource: source));
        }
    }

    public Task<Result<AuthResult>> GetUserAsync(string userId)
    {
        const string source = "Auth0Provider.GetUserAsync";
        try
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                return Task.FromResult(Result<AuthResult>.Success(new AuthResult { UserId = user.UserId, Email = user.Email, Username = user.Username, AccessToken = "current_access_token", Roles = user.Roles, Succeeded = true }));
            }
            return Task.FromResult(Result<AuthResult>.Failure("User not found", errorSource: source));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for user ID {UserId}", source, userId);
            return Task.FromResult(Result<AuthResult>.Failure(ex.Message, errorSource: source));
        }
    }

    public Task<Result<string>> GetAccessTokenAsync()
    {
        const string source = "Auth0Provider.GetAccessTokenAsync";
        try
        {
            // In a real scenario, this would retrieve the current access token from a secure storage
            return Task.FromResult(Result<string>.Success("current_access_token"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source}", source);
            return Task.FromResult(Result<string>.Failure(ex.Message, errorSource: source));
        }
    }

    public Task<Result> UpdateUserProfileAsync(string userId, UpdateUserProfileCommand request)
    {
        const string source = "Auth0Provider.UpdateUserProfileAsync";
        try
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return Task.FromResult(Result.Failure("User not found", errorSource: source));
            }

            if (request.Name != null) user.Username = request.Name;
            if (request.Email != null) user.Email = request.Email;
            // For picture and user_metadata, a real Auth0 API call would be made here.
            // For this mock, we'll just log that they would be updated.
            if (request.Picture != null) _logger.LogInformation("Mock: User {UserId} picture would be updated to {Picture}", userId, request.Picture);
            if (request.UserMetadata != null) _logger.LogInformation("Mock: User {UserId} user_metadata would be updated to {Metadata}", userId, request.UserMetadata);

            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for user ID {UserId}", source, userId);
            return Task.FromResult(Result.Failure(ex.Message, errorSource: source));
        }
    }
}