using backend.Application.Common.Models;
using backend.Application.VoiceProfiles.Queries;

namespace backend.Application.VoiceProfiles.Commands.ImportVoiceProfiles;

/// <summary>
/// Lệnh để nhập danh sách các hồ sơ giọng nói vào một gia đình.
/// </summary>
public record ImportVoiceProfilesCommand : IRequest<Result<Unit>>
{
    /// <summary>
    /// ID của gia đình mà các hồ sơ giọng nói sẽ được nhập vào.
    /// </summary>
    public Guid FamilyId { get; init; }

    /// <summary>
    /// Danh sách các hồ sơ giọng nói để nhập.
    /// </summary>
    public List<VoiceProfileDto> VoiceProfiles { get; init; } = new();
}
