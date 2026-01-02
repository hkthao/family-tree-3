using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.VoiceProfiles.Queries;

/// <summary>
/// Handler để xử lý UpdateVoiceProfileConsentCommand.
/// </summary>
public class UpdateVoiceProfileConsentCommandHandler : IRequestHandler<UpdateVoiceProfileConsentCommand, Result<VoiceProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateVoiceProfileConsentCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<VoiceProfileDto>> Handle(UpdateVoiceProfileConsentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.VoiceProfiles
            .Where(vp => vp.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return Result<VoiceProfileDto>.Failure($"Voice Profile with ID {request.Id} not found.");
        }

        entity.UpdateConsent(request.Consent);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<VoiceProfileDto>.Success(_mapper.Map<VoiceProfileDto>(entity));
    }
}
