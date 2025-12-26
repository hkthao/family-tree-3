namespace backend.Application.AI.DTOs;

using System.Collections.Generic; // Add this
using backend.Application.MemberFaces.Commands.DetectFaces;

public class ChatResponse
{
    public string? Output { get; set; }
    public CombinedAiContentDto? GeneratedData { get; set; }
    public string? Intent { get; set; } // New property for redirection URL

    /// <summary>
    /// Kết quả nhận dạng khuôn mặt, nếu ngữ cảnh là nhận dạng hình ảnh.
    /// </summary>
    public ICollection<FaceDetectionResponseDto>? FaceDetectionResults { get; set; }
}
