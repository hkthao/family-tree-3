using backend.Application.Common.Interfaces;

namespace backend.Infrastructure.Auth;

public class Auth0Config : IAuth0Config
{
    public string? Domain { get; set; }
    public string? Audience { get; set; }
    public string? Namespace { get; set; }
}
