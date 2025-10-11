namespace backend.Infrastructure.Auth;

public class AuthConfig 
{
    public Auth0Config? Auth0 { get; set; }
}

public class Auth0Config 
{
    public string? Domain { get; set; }
    public string? Audience { get; set; }
    public string? Namespace { get; set; }
}
