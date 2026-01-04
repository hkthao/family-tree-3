namespace backend.Application.VoiceProfiles.Queries;

/// <summary>
/// DTO đại diện cho thông tin tạo giọng nói.
/// </summary>
public class VoiceGenerationDto
{
    public Guid Id { get; set; }
    public Guid VoiceProfileId { get; set; }
    public string Text { get; set; } = null!;
    public string AudioUrl { get; set; } = null!;
    public double Duration { get; set; }
    public DateTime Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}
