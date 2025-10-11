using System.Security.Claims;
using backend.Application.Common.Interfaces;

namespace backend.Infrastructure.Auth
{
    public class Auth0ClaimProvider : IExternalClaimProvider
    {
        private readonly IAuth0Config _auth0Config;

        public Auth0ClaimProvider(IAuth0Config auth0Config)
        {
            _auth0Config = auth0Config;
        }

        public string? GetEmail(ClaimsPrincipal principal)
        {
            return principal.FindFirst($"{_auth0Config.Namespace}email")?.Value ?? principal.FindFirst(ClaimTypes.Email)?.Value;
        }

        public string? GetName(ClaimsPrincipal principal)
        {
            return principal.FindFirst($"{_auth0Config.Namespace}name")?.Value ?? principal.FindFirst(ClaimTypes.Name)?.Value;
        }

        public string? GetExternalId(ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
