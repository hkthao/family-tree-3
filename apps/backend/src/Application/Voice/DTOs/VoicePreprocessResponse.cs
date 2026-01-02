using System.Text.Json.Serialization;

namespace backend.Application.Voice.DTOs;

public class VoicePreprocessResponse
{
    [JsonPropertyName("processed-audio-url")]
    public string ProcessedAudioUrl { get; set; } = string.Empty;
    public double Duration { get; set; }
}
