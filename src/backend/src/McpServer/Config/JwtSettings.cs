namespace McpServer.Config;

/// <summary>
/// Represents the configuration settings for JSON Web Token (JWT).
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// The JWT authority.
    /// </summary>
    public string Authority { get; set; } = null!;
    /// <summary>
    /// The JWT audience.
    /// </summary>
    public string Audience { get; set; } = null!;
    /// <summary>
    /// The namespace used in the JWT.
    /// </summary>
    public string Namespace { get; set; } = null!;
}
