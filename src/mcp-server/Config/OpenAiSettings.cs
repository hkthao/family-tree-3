namespace McpServer.Config
{
    public class OpenAiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-3.5-turbo"; // Default OpenAI model
    }
}