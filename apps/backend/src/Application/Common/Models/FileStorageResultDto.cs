namespace backend.Application.Common.Models;

public class FileStorageResultDto
{
    public string FileUrl { get; set; } = string.Empty;
    public string? DeleteHash { get; set; }
}
