using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Auth
{
    public class Auth0ClaimsTransformer : IClaimsTransformation
    {
        private readonly JwtSettings _authConfig;

        public Auth0ClaimsTransformer(IOptions<JwtSettings> authConfig)
        {
            _authConfig = authConfig.Value ?? throw new ArgumentNullException("JwtOptions cannot be null.");
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = (ClaimsIdentity)principal.Identity!;

            // Map Auth0 claims to standard .NET Core claims
            // Example: Map 'name' claim from Auth0 to ClaimTypes.Name
            var nameClaim = identity.FindFirst($"{_authConfig.Namespace}name") ?? identity.FindFirst("name");
            if (nameClaim != null)
            {
                identity.AddClaim(new Claim(ClaimTypes.Name, nameClaim.Value));
            }

            // Example: Map 'email' claim from Auth0 to ClaimTypes.Email
            var emailClaim = identity.FindFirst($"{_authConfig.Namespace}email") ?? identity.FindFirst("email");
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
