using System.Text.Json.Serialization;

namespace backend.Application.Voice.DTOs;

public class VoiceGenerateResponse
{
    [JsonPropertyName("audio-url")]
    public string AudioUrl { get; set; } = string.Empty;
}
