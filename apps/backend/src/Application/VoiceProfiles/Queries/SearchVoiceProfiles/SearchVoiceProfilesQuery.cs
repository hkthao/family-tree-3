using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.VoiceProfiles.Queries.SearchVoiceProfiles;

public record SearchVoiceProfilesQuery : PaginatedQuery, IRequest<PaginatedList<VoiceProfileDto>>
{
    public Guid FamilyId { get; set; }
    public string? SearchQuery { get; init; }
    public Guid? MemberId { get; init; }
    public VoiceProfileStatus? Status { get; init; }
}
