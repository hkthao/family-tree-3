namespace backend.Application.Voice.DTOs;

public class VoiceGenerateRequest
{
    public string SpeakerWavUrl { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}