using backend.Application.Common.Models;

namespace backend.Application.VoiceProfiles.Commands.DeleteVoiceProfile;

public record DeleteVoiceProfileCommand(Guid Id) : IRequest<Result>;
