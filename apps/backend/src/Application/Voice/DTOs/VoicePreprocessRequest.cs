namespace backend.Application.Voice.DTOs;

public class VoicePreprocessRequest
{
    public List<string> AudioUrls { get; set; } = new List<string>();
}
