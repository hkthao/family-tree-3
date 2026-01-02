using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.VoiceProfiles.Queries;

/// <summary>
/// Handler để xử lý ArchiveVoiceProfileCommand.
/// </summary>
public class ArchiveVoiceProfileCommandHandler : IRequestHandler<ArchiveVoiceProfileCommand, Result<VoiceProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ArchiveVoiceProfileCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<VoiceProfileDto>> Handle(ArchiveVoiceProfileCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.VoiceProfiles
            .Where(vp => vp.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return Result<VoiceProfileDto>.Failure($"Voice Profile with ID {request.Id} not found.");
        }

        entity.Archive();

        await _context.SaveChangesAsync(cancellationToken);

        return Result<VoiceProfileDto>.Success(_mapper.Map<VoiceProfileDto>(entity));
    }
}
