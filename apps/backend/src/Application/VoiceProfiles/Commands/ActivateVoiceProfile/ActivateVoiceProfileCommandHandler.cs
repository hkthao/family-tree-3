using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.VoiceProfiles.Queries;

namespace backend.Application.VoiceProfiles.Commands.ActivateVoiceProfile;

/// <summary>
/// Handler để xử lý ActivateVoiceProfileCommand.
/// </summary>
public class ActivateVoiceProfileCommandHandler : IRequestHandler<ActivateVoiceProfileCommand, Result<VoiceProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ActivateVoiceProfileCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<VoiceProfileDto>> Handle(ActivateVoiceProfileCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.VoiceProfiles
            .Where(vp => vp.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return Result<VoiceProfileDto>.Failure($"Voice Profile with ID {request.Id} not found.");
        }

        entity.Activate();

        await _context.SaveChangesAsync(cancellationToken);

        return Result<VoiceProfileDto>.Success(_mapper.Map<VoiceProfileDto>(entity));
    }
}
