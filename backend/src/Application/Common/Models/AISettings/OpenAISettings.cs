namespace backend.Application.Common.Models.AISettings
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Organization { get; set; } = null!;
    }
}
