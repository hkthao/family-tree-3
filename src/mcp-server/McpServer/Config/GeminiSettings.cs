namespace McpServer.Config
{
    public class GeminiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string ModelId { get; set; } = "gemini-pro";
    }
}