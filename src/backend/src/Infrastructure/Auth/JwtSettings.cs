namespace backend.Infrastructure.Auth;

public class JwtSettings
{
    public string Authority { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string Namespace { get; set; } = null!;
}
