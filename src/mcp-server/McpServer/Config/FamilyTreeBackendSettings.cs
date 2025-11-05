namespace McpServer.Config
{
    public class FamilyTreeBackendSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string JwtAudience { get; set; } = string.Empty;
        public string JwtIssuer { get; set; } = string.Empty;
        // Note: For a real-world scenario, you would typically validate JWTs using the public key
        // from the Identity Provider (e.g., Auth0, your main backend's JWKS endpoint).
        // For simplicity, if the main backend uses a symmetric key, you'd put it here.
        // Otherwise, you'd configure options.MetadataAddress or options.ConfigurationManager.
        public string JwtSecretKey { get; set; } = string.Empty; // Only for symmetric key validation
    }
}