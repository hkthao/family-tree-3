using backend.Application.Common.Models;
using backend.Application.VoiceProfiles.Queries;

/// <summary>
/// Command để cập nhật trạng thái đồng ý của hồ sơ giọng nói.
/// </summary>
public record UpdateVoiceProfileConsentCommand : IRequest<Result<VoiceProfileDto>>
{
    /// <summary>
    /// ID của hồ sơ giọng nói cần cập nhật.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Trạng thái đồng ý mới.
    /// </summary>
    public bool Consent { get; init; }
}
