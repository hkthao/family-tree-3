namespace backend.Application.Common.Models.ImageRestoration;

public class PreprocessImageResponseDto
{
    public string ProcessedImageBase64 { get; set; } = string.Empty;
    public bool IsResized { get; set; }
}
