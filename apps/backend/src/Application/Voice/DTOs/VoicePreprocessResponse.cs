using System.Text.Json.Serialization;

namespace backend.Application.Voice.DTOs;

public class VoicePreprocessResponse
{
    [JsonPropertyName("processed_audio_url")]
    public string ProcessedAudioUrl { get; set; } = string.Empty;
    public double Duration { get; set; }
}
