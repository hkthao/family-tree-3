using System.Text.Json.Serialization;

namespace backend.Application.VoiceProfiles.DTOs;

public class VoiceGenerateResponse
{
    [JsonPropertyName("audio-url")]
    public string AudioUrl { get; set; } = string.Empty;
}
