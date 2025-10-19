using System.Security.Claims;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AppSetting;
using Microsoft.AspNetCore.Authentication;

namespace backend.Infrastructure.Auth
{
    public class Auth0ClaimsTransformer : IClaimsTransformation
    {
        private readonly IConfigProvider _configProvider;

        public Auth0ClaimsTransformer(IConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = (ClaimsIdentity)principal.Identity!;
            var authConfig = _configProvider.GetSection<JwtSettings>();

            // Map Auth0 claims to standard .NET Core claims
            // Example: Map 'name' claim from Auth0 to ClaimTypes.Name
            var nameClaim = identity.FindFirst($"{authConfig.Namespace}name") ?? identity.FindFirst("name");
            if (nameClaim != null)
            {
                identity.AddClaim(new Claim(ClaimTypes.Name, nameClaim.Value));
            }

            // Example: Map 'email' claim from Auth0 to ClaimTypes.Email
            var emailClaim = identity.FindFirst($"{authConfig.Namespace}email") ?? identity.FindFirst("email");
            if (emailClaim != null)
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, emailClaim.Value));
            }

            // Example: Map 'sub' claim from Auth0 to ClaimTypes.NameIdentifier
            var subClaim = identity.FindFirst("sub");
            if (subClaim != null)
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, subClaim.Value));
            }

            return Task.FromResult(principal);
        }
    }
}
