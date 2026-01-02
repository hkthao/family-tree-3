using backend.Application.Common.Models;

namespace backend.Application.VoiceProfiles.Queries.GetVoiceProfilesByMemberId;

/// <summary>
/// Query để lấy danh sách các hồ sơ giọng nói theo ID thành viên.
/// </summary>
public record GetVoiceProfilesByMemberIdQuery : IRequest<Result<List<VoiceProfileDto>>>
{
    /// <summary>
    /// ID của thành viên cần lấy hồ sơ giọng nói.
    /// </summary>
    public Guid MemberId { get; init; }
}
