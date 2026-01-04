using backend.Application.Common.Models;
using backend.Application.VoiceProfiles.Queries;

namespace backend.Application.VoiceProfiles.Commands.PreprocessAndCreateVoiceProfile;

public record PreprocessAndCreateVoiceProfileCommand : IRequest<Result<VoiceProfileDto>>
{
    public Guid MemberId { get; init; }
    public string Label { get; init; } = null!;
    public List<string> RawAudioUrls { get; init; } = new List<string>();
    public string Language { get; init; } = null!;
    public bool Consent { get; init; }
}
