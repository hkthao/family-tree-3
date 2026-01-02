using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.VoiceProfiles.Queries;
using backend.Domain.Entities;

namespace backend.Application.VoiceProfiles.Commands.CreateVoiceProfile;

/// <summary>
/// Handler để xử lý CreateVoiceProfileCommand.
/// </summary>
public class CreateVoiceProfileCommandHandler : IRequestHandler<CreateVoiceProfileCommand, Result<VoiceProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateVoiceProfileCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<VoiceProfileDto>> Handle(CreateVoiceProfileCommand request, CancellationToken cancellationToken)
    {
        var entity = new VoiceProfile(
            request.MemberId,
            request.Label,
            request.AudioUrl,
            request.DurationSeconds,
            request.Language,
            request.Consent
        );

        _context.VoiceProfiles.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<VoiceProfileDto>.Success(_mapper.Map<VoiceProfileDto>(entity));
    }
}
