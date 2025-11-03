using System.Security.Claims;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AppSetting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Auth
{
    public class Auth0ClaimsTransformer(IConfigProvider configProvider, ILogger<Auth0ClaimsTransformer> logger) : IClaimsTransformation
    {
        private readonly IConfigProvider _configProvider = configProvider;
        private readonly ILogger<Auth0ClaimsTransformer> _logger = logger;

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            _logger.LogInformation("IClaimsTransformation running for user");
            var identity = (ClaimsIdentity)principal.Identity!;
            var authConfig = _configProvider.GetSection<JwtSettings>();

            // Map Auth0 claims to standard .NET Core claims
            // Example: Map 'name' claim from Auth0 to ClaimTypes.Name
            var nameClaim = identity.FindFirst("name") ?? identity.FindFirst($"{authConfig.Namespace}name");
            if (nameClaim != null)
            {
                _logger.LogInformation("IClaimsTransformation running for nameClaim {Value}", nameClaim.Value);
                identity.AddClaim(new Claim(ClaimTypes.Name, nameClaim.Value));
            }

            // Example: Map 'email' claim from Auth0 to ClaimTypes.Email
            var emailClaim = identity.FindFirst("email") ?? identity.FindFirst($"{authConfig.Namespace}email");
            if (emailClaim != null)
            {
                _logger.LogInformation("IClaimsTransformation running for emailClaim {Value}", emailClaim.Value);
                identity.AddClaim(new Claim(ClaimTypes.Email, emailClaim.Value));
            }

            // Example: Map 'sub' claim from Auth0 to ClaimTypes.NameIdentifier
            var subClaim = identity.FindFirst("sub");
            if (subClaim != null)
            {
                _logger.LogInformation("IClaimsTransformation running for subClaim {Value}", subClaim.Value);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, subClaim.Value));
            }

            return Task.FromResult(principal);
        }
    }
}
