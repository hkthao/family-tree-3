namespace McpServer.Config
{
    public class LocalLlmSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty; // e.g., "llama2"
    }
}