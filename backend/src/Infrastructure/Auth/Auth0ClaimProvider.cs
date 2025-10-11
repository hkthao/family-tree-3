using System.Security.Claims;
using backend.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Auth
{
    public class Auth0ClaimProvider : IExternalClaimProvider
    {
        private readonly Auth0Config _authConfig;

        public Auth0ClaimProvider(IOptions<AuthConfig> authConfig)
        {
            _authConfig = authConfig.Value.Auth0 ?? throw new ArgumentNullException("AuthConfig.Auth0 cannot be null.");
        }

        public string? GetEmail(ClaimsPrincipal principal)
        {
            return principal.FindFirst($"{_authConfig.Namespace}email")?.Value ?? principal.FindFirst(ClaimTypes.Email)?.Value;
        }

        public string? GetName(ClaimsPrincipal principal)
        {
            return principal.FindFirst($"{_authConfig.Namespace}name")?.Value ?? principal.FindFirst(ClaimTypes.Name)?.Value;
        }

        public string? GetExternalId(ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
