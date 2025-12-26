namespace backend.Application.Common.Models;

public class ImageUploadResultDto
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Url { get; set; }
    public string? DeleteUrl { get; set; }
    public string? MimeType { get; set; }
    public long Size { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
