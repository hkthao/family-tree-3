namespace backend.Application.Voice.DTOs;

public class VoicePreprocessResponse
{
    public string ProcessedAudioUrl { get; set; } = string.Empty;
    public double Duration { get; set; }
}
