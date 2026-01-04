using System.Text.Json.Serialization; // Added for JsonPropertyName

namespace backend.Application.VoiceProfiles.DTOs; // Corrected namespace

public class VoiceGenerateRequest
{
    [JsonPropertyName("speaker_wav_url")]
    public string SpeakerWavUrl { get; set; } = string.Empty;
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;
}
