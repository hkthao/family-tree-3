using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.VoiceProfiles.Queries.GetVoiceProfileById;

/// <summary>
/// Handler để xử lý GetVoiceProfileByIdQuery.
/// </summary>
public class GetVoiceProfileByIdQueryHandler : IRequestHandler<GetVoiceProfileByIdQuery, Result<VoiceProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetVoiceProfileByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<VoiceProfileDto>> Handle(GetVoiceProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.VoiceProfiles
            .Where(vp => vp.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Domain.Entities.VoiceProfile), request.Id);
        }

        return Result<VoiceProfileDto>.Success(_mapper.Map<VoiceProfileDto>(entity));
    }
}
