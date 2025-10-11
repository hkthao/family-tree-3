namespace backend.Infrastructure.Auth;

public class AuthConfig
{
  public string Authority { get; set; } = null!;
  public string Audience { get; set; } = null!;
  public Auth0Config Auth0 { get; set; } = null!;
}

public class Auth0Config
{
  public string Namespace { get; set; } = null!;
}