using backend.Application.Common.Models;

namespace backend.Application.VoiceProfiles.Queries.GetVoiceGenerationHistoryByMemberId;

/// <summary>
/// Query to retrieve voice generation history for a specific member.
/// </summary>
public record GetVoiceGenerationHistoryByMemberIdQuery : IRequest<Result<List<VoiceGenerationDto>>>
{
    /// <summary>
    /// The ID of the member to retrieve voice generation history for.
    /// </summary>
    public Guid MemberId { get; init; }
}
