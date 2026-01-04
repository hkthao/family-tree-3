using System.Text.Json.Serialization;

namespace backend.Application.Voice.DTOs;

public class AudioQualityReportDto
{
    [JsonPropertyName("overall_quality")]
    public string OverallQuality { get; set; } = string.Empty;

    [JsonPropertyName("quality_score")]
    public double QualityScore { get; set; }

    [JsonPropertyName("messages")]
    public List<string> Messages { get; set; } = new List<string>();
}

public class VoicePreprocessResponse
{
    [JsonPropertyName("processed_audio_url")]
    public string ProcessedAudioUrl { get; set; } = string.Empty;

    public double Duration { get; set; }

    [JsonPropertyName("quality_report")]
    public AudioQualityReportDto QualityReport { get; set; } = new AudioQualityReportDto();
}
