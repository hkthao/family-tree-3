using System.Security.Claims;

namespace backend.Application.Common.Interfaces
{
    public interface IExternalClaimProvider
    {
        string? GetEmail(ClaimsPrincipal principal);
        string? GetName(ClaimsPrincipal principal);
        string? GetExternalId(ClaimsPrincipal principal);
    }
}
