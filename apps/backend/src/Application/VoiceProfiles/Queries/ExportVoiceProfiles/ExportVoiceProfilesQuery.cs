using backend.Application.Common.Models;
using backend.Application.VoiceProfiles.Queries;
using MediatR;

namespace backend.Application.VoiceProfiles.Queries.ExportVoiceProfiles;

/// <summary>
/// Truy vấn để xuất danh sách các hồ sơ giọng nói của một thành viên.
/// </summary>
public record ExportVoiceProfilesQuery : IRequest<Result<List<VoiceProfileDto>>>
{
    /// <summary>
    /// ID của gia đình mà các hồ sơ giọng nói sẽ được xuất.
    /// </summary>
    public Guid FamilyId { get; init; }
}
