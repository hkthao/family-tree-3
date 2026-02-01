namespace backend.Application.Common.Models;

public class OcrResultDto
{
    public string Text { get; set; } = string.Empty;
    public bool Success { get; set; } // NEW
    public byte[]? ProcessedBytes { get; set; } // For potentially returning processed image/pdf
    public string? Error { get; set; }
}
