using backend.Domain.Enums;

namespace backend.Application.FamilyMedia.DTOs;

public class FamilyMediaDto
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailPath { get; set; }
    public Guid? UploadedBy { get; set; }
    public string? UploadedByName { get; set; } // Display name of the uploader
    public DateTime Created { get; set; }

    public ICollection<MediaLinkDto> MediaLinks { get; set; } = new List<MediaLinkDto>();
}
