namespace backend.Application.Memories.DTOs;

public class CropAndAnalyzeFaceResponseDto
{
    public string CroppedFaceBase64 { get; set; } = string.Empty;
    public string Emotion { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public string? MemberId { get; set; }
}
