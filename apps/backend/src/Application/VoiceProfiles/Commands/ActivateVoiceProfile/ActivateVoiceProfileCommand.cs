using backend.Application.Common.Models;
using backend.Application.VoiceProfiles.Queries;

namespace backend.Application.VoiceProfiles.Commands.ActivateVoiceProfile;

/// <summary>
/// Command để kích hoạt lại một hồ sơ giọng nói.
/// </summary>
public record ActivateVoiceProfileCommand : IRequest<Result<VoiceProfileDto>>
{
    /// <summary>
    /// ID của hồ sơ giọng nói cần kích hoạt.
    /// </summary>
    public Guid Id { get; init; }
}
