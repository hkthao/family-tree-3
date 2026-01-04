using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.VoiceProfiles.Queries;

/// <summary>
/// Handler để xử lý UpdateVoiceProfileCommand.
/// </summary>
public class UpdateVoiceProfileCommandHandler : IRequestHandler<UpdateVoiceProfileCommand, Result<VoiceProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateVoiceProfileCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<VoiceProfileDto>> Handle(UpdateVoiceProfileCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.VoiceProfiles
            .Where(vp => vp.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return Result<VoiceProfileDto>.Failure($"Voice Profile with ID {request.Id} not found.");
        }

        entity.Update(
            request.Label,
            request.AudioUrl,
            request.DurationSeconds,
            request.QualityScore,
            request.OverallQuality,
            request.QualityMessages,
            request.Language,
            request.Consent,
            request.Status
        );

        await _context.SaveChangesAsync(cancellationToken);

        return Result<VoiceProfileDto>.Success(_mapper.Map<VoiceProfileDto>(entity));
    }
}
