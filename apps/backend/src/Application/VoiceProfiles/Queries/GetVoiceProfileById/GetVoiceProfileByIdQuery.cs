using backend.Application.Common.Models;

namespace backend.Application.VoiceProfiles.Queries.GetVoiceProfileById;

/// <summary>
/// Query để lấy thông tin chi tiết của một hồ sơ giọng nói theo ID.
/// </summary>
public record GetVoiceProfileByIdQuery : IRequest<Result<VoiceProfileDto>>
{
    /// <summary>
    /// ID của hồ sơ giọng nói cần lấy.
    /// </summary>
    public Guid Id { get; init; }
}
