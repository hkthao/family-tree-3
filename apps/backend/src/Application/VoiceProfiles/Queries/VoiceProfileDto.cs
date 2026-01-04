using backend.Domain.Enums;

namespace backend.Application.VoiceProfiles.Queries;

/// <summary>
/// DTO đại diện cho thông tin hồ sơ giọng nói.
/// </summary>
public class VoiceProfileDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string Label { get; set; } = null!;
    public string AudioUrl { get; set; } = null!;
    public double DurationSeconds { get; set; }
    public string Language { get; set; } = null!;
    public bool Consent { get; set; }
    public VoiceProfileStatus Status { get; set; }
    public DateTime Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}
