namespace backend.Application.Files.Queries;

public class FileResultDto
{
    public string FileName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
}
