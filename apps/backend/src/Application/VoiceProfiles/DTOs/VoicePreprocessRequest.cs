using System.Text.Json.Serialization;

namespace backend.Application.Voice.DTOs;

public class VoicePreprocessRequest
{
    [JsonPropertyName("audio_urls")]
    public List<string> AudioUrls { get; set; } = new List<string>();
}
