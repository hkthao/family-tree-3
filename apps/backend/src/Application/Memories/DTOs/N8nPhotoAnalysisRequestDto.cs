using System.Text.Json; // For JsonDocument

namespace backend.Application.Memories.DTOs;

public class N8nPhotoAnalysisRequestDto
{
    public string CroppedFaceBase64 { get; set; } = string.Empty;
    public JsonDocument Faces { get; set; } = JsonDocument.Parse("{}"); // All detected face locations
    public Guid? MemberId { get; set; }
    public string Emotion { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public FaceLocationDto FirstFaceLocation { get; set; } = new FaceLocationDto(); // The specific face location chosen for cropping
}
