using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Identity.Commands.UpdateUserProfile;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Auth
{
    public class Auth0Provider : IAuthProvider
    {
        private readonly ILogger<Auth0Provider> _logger;
        private readonly IConfiguration _configuration;

        public Auth0Provider(ILogger<Auth0Provider> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        private async Task<ManagementApiClient> GetManagementApiClientAsync()
        {
            var auth0Domain = _configuration["Auth0:Domain"];
            var m2mClientId = _configuration["Auth0:ClientId"];
            var m2mClientSecret = _configuration["Auth0:ClientSecret"];
            var audience = _configuration["Auth0:Audience"];

            if (string.IsNullOrEmpty(auth0Domain) || string.IsNullOrEmpty(m2mClientId) || string.IsNullOrEmpty(m2mClientSecret) || string.IsNullOrEmpty(audience))
            {
                _logger.LogError("Auth0 configuration is missing or invalid. Check Auth0:Domain, Auth0:ClientId, Auth0:ClientSecret, and Auth0:Audience in settings.");
                throw new InvalidOperationException("Auth0 configuration is missing or invalid.");
            }

            var authClient = new AuthenticationApiClient(auth0Domain);
            var tokenRequest = new ClientCredentialsTokenRequest
            {
                ClientId = m2mClientId,
                ClientSecret = m2mClientSecret,
                Audience = audience
            };

            var tokenResponse = await authClient.GetTokenAsync(tokenRequest);
            return new ManagementApiClient(tokenResponse.AccessToken, auth0Domain);
        }

        public async Task<Result> UpdateUserProfileAsync(string userId, UpdateUserProfileCommand request)
        {
            const string source = "Auth0Provider.UpdateUserProfileAsync";
            try
            {
                var managementClient = await GetManagementApiClientAsync();

                var userUpdateRequest = new UserUpdateRequest();

                if (request.Name != null)
                {
                    userUpdateRequest.FullName = request.Name;
                    userUpdateRequest.NickName = request.Name;
                }
                if (request.Email != null)
                {
                    userUpdateRequest.Email = request.Email;
                }
                if (request.Picture != null)
                {
                    userUpdateRequest.Picture = request.Picture;
                }
                if (request.UserMetadata != null)
                {
                    userUpdateRequest.UserMetadata = request.UserMetadata;
                }


                await managementClient.Users.UpdateAsync(userId, userUpdateRequest);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Source} for user ID {UserId}", source, userId);
                return Result.Failure(ex.Message, errorSource: source);
            }
        }

        // --- To be Implemented ---

        public Task<Result<AuthResult>> LoginAsync(string email, string password)
        {
            _logger.LogWarning("LoginAsync is not implemented.");
            throw new NotImplementedException();
        }

        public Task<Result<AuthResult>> RegisterAsync(string email, string password, string username)
        {
            _logger.LogWarning("RegisterAsync is not implemented.");
            throw new NotImplementedException();
        }

        public Task<Result<AuthResult>> GetUserAsync(string userId)
        {
            _logger.LogWarning("GetUserAsync is not implemented.");
            throw new NotImplementedException();
        }

        public Task<Result<string>> GetAccessTokenAsync()
        {
            _logger.LogWarning("GetAccessTokenAsync is not implemented.");
            throw new NotImplementedException();
        }
    }
}